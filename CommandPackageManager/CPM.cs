/*
 *  Copyright 2021, Timothy Leitzke/GrimOfDoom

    GPL-V3

    This file is part of OCAC (Open Center API Controls).

    OCAC is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    Timothy Leitzke, version 3 of the License.

    OCAC is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Reflection;

namespace CPM {
    public class Core {
        /// <summary>Fast access list of all commands across all packages
        public Dictionary<string, Command> callStack = new();
        /// <summary>All packages found & added</summary>
        public List<Package> packages = new();

        public Core() {
            Initialize();
        }

        /// <summary>Call an action to run by its callStack dictionary name</summary>
        public object Perform(RecievedCommand cmd) {
            if (callStack.ContainsKey(cmd.command)){
                return callStack[cmd.command].Perform(cmd.values);
            } else {
                Console.WriteLine("Action does not exist: " + cmd.command);
                return "Command does not exist: " + cmd.command;
            }
        }

        /// <summary>From scratch, reinitialize commands. Useful to look for new packages while running</summary>
        public void Initialize() {
            //Reset packages and callStack
            callStack = new Dictionary<string, Command>();
            packages = new List<Package>();

            //Find all packages
            List<Assembly> newPackages = new List<Assembly>();
            string binFolder = Directory.GetCurrentDirectory() + "\\plugins";

            foreach (string dll in Directory.GetFiles(binFolder, "*.dll")) {
                newPackages.Add(Assembly.LoadFile(dll));
            }

            //Process each found .dll
            foreach (Assembly newPackage in newPackages) {
                Console.WriteLine("assembly attempt: " + newPackage.FullName);
                try {
                    //If package exists in assembly, add to packages
                    Type? foundPackage = newPackage.GetType("PackageExtension");
                    if (foundPackage != null) {
                        //Try and create instance of package to work with
                        object? classInstance = Activator.CreateInstance(foundPackage);
                        if (classInstance != null) {
                            packages.Add((CPM.Package)classInstance);
                        } else {
                            throw new Exception("Failed instancing package: " + newPackage.FullName);
                        }
                    } else {
                        throw new Exception("No package found " + newPackage.Location);
                    }
                } catch (Exception e){
                    //Something went wrong
                    Console.WriteLine("Package Error: " + e);
                }
            }

            //Add commands to callStack
            foreach (Package package in packages) {
                foreach (Command cmd in package.commands) {
                    callStack[package.callName + "." + cmd.callName] = cmd;
                    cmd.Initialize();
                }
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

    /// <summary>A single container holding a set of similar commands</summary>
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

    /// <summary>A single command/action that can be called to perform by frontend</summary>
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

        /// <summary>What values are expected to be passed to this command</summary>
        public List<ExpectedValues> expectedValues = new();

        /// <summary>Resources that are available for this command</summary>
        public List<AvailableResources> availableResources = new();

        /// <summary>This is the method ran when command is called</summary>
        public virtual object Perform(List<CommandValue> values) {
            return null;
        }

        /// <summary>This is the method ran when the program is first loaded up</summary>
        public virtual void Initialize() {

        }
    }

    public record ExpectedValues {
        /// <summary>Case Sensitive name of the value</summary>
        public string? name { get; set; }

        /// <summary>Variable type that should be passed through
        /// <para>Ex: URL, INT, FLOAT, STRING</para></summary>
        public string? varType { get; set; }
        /// <summary>What should the value represent
        /// <para>Ex: URL to image</para></summary>
        public string? description { get; set; }
    }

    public record AvailableResources {
        /// <summary>Name of the resource</summary>
        public string? name { get; set; }

        /// <summary>What type of resource is this, file type endings when possible</summary>
        public string? resourceType { get; set; }

        /// <summary>Folder, URL, ETC location for gaining access to the resource</summary>
        public string? resourceLocation { get; set; }

    }

    /// <summary>Command recieved by frontend</summary>
    public class RecievedCommand {
        /// <summary>String value command</summary>
        public string? command { get; set; }

        /// <summary>Values for command</summary>
        public List<CommandValue>? values { get; set; }
    }

    /// <summary>Values sent by frontend</summary>
    public class CommandValue {
        public string? name { get; set; }
        public string? value { get; set; }
    }

    public class OK {
        string status = "OK";
    }
}
