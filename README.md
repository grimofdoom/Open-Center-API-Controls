[Download Builds](https://grimofdoom.itch.io/ocac)

# Open-Center-API-Controls
An open source project dedicated to competing with Elgato Streamdeck, and taking things steps ahead.

    Copyright 2021, Timothy Leitzke/GrimOfDoom

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
    
    
OCAC Read.me

How to use:  
Once built run Open Center API Controls.exe  
In browser of any device, go to [https://hostdeviceIP:7072/] to access built in front end  
Settings controls visual layout/appearance  
Modify gives control over individual buttons  
-Once you open Modify, press the button you wish to modify  
-Make the desired changes, and press button at bottom to commit (Permenant save does not exist, yet)  
-Close modify tab  
-Press buttons as setup to control host computer


API Requests:

GET:/  
    -Home screen for built in frontend, easily replacable by creating or using another frontend  

GET:/CommandData  
    -Recieve an object containing all plugins/commands and necessary information for frontend to run  
  
POST:/Command [ "command" : "", "values": [ "name" : "", "value" : ""]]  
    -Send a command with expected values, defined in CommandData object  
    -MUST(?) include both a "command" value and a "values" array.  
