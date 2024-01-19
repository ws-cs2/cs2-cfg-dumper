using System.Diagnostics;
using System.Globalization;
using System.Text;
using SteamDatabase.ValvePak;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.Serialization;
using ValveResourceFormat.Utils;

using var package = new Package();

// C:\Program Files (x86)\Steam\steamapps\workshop\content\730
var cs2WorkshopContentDir = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\730";
var dumpOutputDir = @"C:\dev\cs2-cfg-dumper\output";

// make sure the directory exists
Directory.CreateDirectory(dumpOutputDir);


// list all folders in the directory
var folders = Directory.GetDirectories(cs2WorkshopContentDir);

// loop through all folders
foreach (var folder in folders)
{
    // get the folder name
    var folderName = Path.GetFileName(folder);

    // get the pak file
    var pakFile = Directory.GetFiles(folder, "*.vpk").FirstOrDefault();

    // if there is no pak file, skip this folder
    if (pakFile == null)
    {
        Console.WriteLine($"No pak file found in {folderName}");
        continue;
    }
    
    Console.WriteLine("Processing {0}", pakFile);

    // open the vpk file
    package.Read(pakFile);
    
    // find all .cfg files
    var cfgDir = package.Entries["cfg"];


    string mapName = "";
    string cfgContents = "";

    foreach (var cfg in cfgDir)
    {
        var fileName = cfg.GetFileName();
        
        if (fileName.EndsWith(".cfg"))
        {
            Console.WriteLine("Found cfg file: {0}", fileName);
            
            package.ReadEntry(cfg, out byte[] contents);
            
            cfgContents = Encoding.UTF8.GetString(contents);
            
            Console.WriteLine(cfgContents);
            
            mapName = fileName.Replace(".cfg", "");
        }
        else
        {
            throw new Exception("Unexpected file in cfg directory");
        }
    }
    
    var map = package.FindEntry("maps/" + mapName + ".vpk");
    
    if (map == null)
    {
        Console.WriteLine("Map not found: {0}", mapName);
        continue;
    }
    
    package.ReadEntry(map, out byte[] mapContents);
    
    var mapContentsStream = new MemoryStream(mapContents);
    
    var mapPackage = new Package();
    mapPackage.SetFileName(mapName + ".vpk");
    mapPackage.Read(mapContentsStream);
    
    var defaultEnts = mapPackage.FindEntry("maps/" + mapName + "/entities/default_ents.vents_c");
    
    if (defaultEnts == null)
    {
        Console.WriteLine("default_ents not found in map: {0}", mapName);
        continue;
    }
    
    mapPackage.ReadEntry(defaultEnts, out byte[] defaultEntsContents);
    
    using var ms = new MemoryStream(defaultEntsContents);
    using var resource = new Resource();
    resource.Read(ms);

    Debug.Assert(resource.ResourceType == ResourceType.EntityLump);

    var entityLump = (EntityLump)resource.DataBlock;


    var entities = entityLump.GetEntities();
    
    bool hasPointServerCommand = false;
    var builder = new StringBuilder();

    foreach (var entity in entities)
    {
        var className = entity.GetProperty<string>("classname");

        if (className == "point_servercommand")
        {
            hasPointServerCommand = true;
        }

        if (className == "logic_auto")
        {
            foreach (var connection in entity.Connections)
            {
                // stolen from: https://github.com/ValveResourceFormat/ValveResourceFormat/blob/928c84bfa5fdc7abec34e6cf2a10a71548e72f44/ValveResourceFormat/Resource/ResourceTypes/EntityLump.cs#L273
                
                builder.Append('@');
                builder.Append(connection.GetProperty<string>("m_outputName"));
                builder.Append(' ');

                var delay = connection.GetFloatProperty("m_flDelay");

                if (delay > 0)
                {
                    builder.Append(CultureInfo.InvariantCulture, $"Delay={delay} ");
                }

                var timesToFire = connection.GetInt32Property("m_nTimesToFire");

                if (timesToFire == 1)
                {
                    builder.Append("OnlyOnce ");
                }
                else if (timesToFire != -1)
                {
                    throw new UnexpectedMagicException("Unexpected times to fire", timesToFire, nameof(timesToFire));
                }

                builder.Append(connection.GetProperty<string>("m_inputName"));
                builder.Append(' ');
                builder.Append(connection.GetProperty<string>("m_targetName"));

                var param = connection.GetProperty<string>("m_overrideParam");

                if (!string.IsNullOrEmpty(param) && param != "(null)")
                {
                    builder.Append(' ');
                    builder.Append(param);
                }

                builder.AppendLine();
            }
        }
    }

    if (!mapName.StartsWith("surf"))
    {
        continue;
    }


    var fullOutput = new StringBuilder();
    
    fullOutput.AppendLine("MAP " + mapName);
    fullOutput.AppendLine("--------------------");
    fullOutput.AppendLine("CFG");
    fullOutput.AppendLine(cfgContents);
    fullOutput.AppendLine("--------------------");
    fullOutput.AppendLine("HAS POINT_SERVERCOMMAND: " + hasPointServerCommand);
    fullOutput.AppendLine("--------------------");
    fullOutput.AppendLine("LOGIC_AUTO");
    fullOutput.AppendLine(builder.ToString());
    fullOutput.AppendLine("--------------------");


    // put in same dir as sourcecode
    var outputDirectory = dumpOutputDir;

    File.WriteAllText(outputDirectory + "\\" + mapName + ".txt", fullOutput.ToString());
}

