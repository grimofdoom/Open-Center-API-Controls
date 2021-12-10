using System.Reflection;

namespace CPM {
    public class Core {
        public Dictionary<string, Command> callStack = new();
        public List<Package> packages = new();

        public Core() {
            Initialize();
        }

        /// <summary>Call an action to run by its callStack dictionary name</summary>
        public void Perform(string actionName) {
            callStack[actionName].Perform();
        }

        /// <summary>
        /// From scratch, reinitialize commands. Useful to look for new packages while running
        /// </summary>
        public void Initialize() {
            //Reset packages and callStack
            callStack = new Dictionary<string, Command>();
            packages = new List<Package>();

            //Find all packages
            List<Assembly> newPackages = new List<Assembly>();
            string binFolder = Directory.GetCurrentDirectory() + "\\bin";

            foreach (string dll in Directory.GetFiles(binFolder, "*.dll")) {
                newPackages.Add(Assembly.LoadFile(dll));
            }

            foreach (Assembly newPackage in newPackages) {
                try {
                    //If package exists in assembly, add to packages
                } catch {
                    //Something went wrong
                }
            }

            //Add commands to callStack
            foreach (Package p in packages) {
                AddPackage(p);
            }
        }

        public void AddPackage(Package package) {
            foreach (Command cmd in package.commands) {
                callStack[package.callName + "." + cmd.callName] = cmd;
                cmd.Initialize();
            }
        }

        /// <summary>Add package without re-initializing all packages</summary>
        public void AddLatePackate(Package package) {
            packages.Add(package);
            foreach (Command cmd in package.commands) {
                callStack[cmd.callName + "." + "p."] = cmd;
                cmd.Initialize();
            }
        }
    }

    public class Package {
        /// <summary>Name of the package</summary>
        public string name = "Default Package 2.1";

        /// <summary>Unique name for package
        /// <para>
        ///     Example: JohnDoe.Package.3.2
        /// </para></summary>
        public string callName = "defaultPackage.2.1";

        /// <summary>The name of whoever has worked on the packages</summary>
        public string author = "GrimOfDoom";

        /// <summary>Description about the package as a whole </summary>
        public string description = "A default description about the entire package.";

        /// <summary>List of all commands that can be performed on call</summary>
        public List<Command> commands = new();
    }

    public class Command {
        /// <summary>Descriptive name of the command</summary>
        public string name = "Default command name";

        /// <summary>The UNIQUE name to the command, used to call the command
        /// <para>
        ///     Example: DefaultCommandName.
        /// </para></summary>
        public string callName = "DefaultCommandName";

        /// <summary>Description of the command</summary>
        public string description = "A default description about an empty command.";

        /// <summary>This is the method ran when command is called</summary>
        public void Perform() {
            
        }

        /// <summary>This is the method ran when the program is first loaded up</summary>
        public void Initialize() {

        }
    }
}
