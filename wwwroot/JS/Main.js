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


var r = document.querySelector(":root");
//Controller Variables
var allButtons = []; //All buttons in array
var ButtonTemplate = document.getElementById("ControllerButtonTemplate");


//Settings Variables
var menusHidden = false;
var settingsShrunk = false;

//Appearance settings
var columns = 5;
var buttonSize = 100;
var gap = 0;
var cornerRadius = 5;
var borderWidth = 1;

var buttonCount = 15;

//Button Control settings
var buttonControlOpen = false;
var selectedButton = -1;
var allPlugins = null;
var setButtons = [];

var pluginSelector = document.getElementById("PluginSelector");
var commandSelector = document.getElementById("CommandSelector");
var commandDesc = document.getElementById("CommandDesc");

//-------------------------------- Page Startup--------------------
for (var i = 0; i < 15; i++) {
    AddButton();
}

//Retrieve and set all plugins/commands
const xhrInitCommands = new XMLHttpRequest();
xhrInitCommands.open("GET", "/CommandData");
xhrInitCommands.send();
xhrInitCommands.onload = () => {
    if (xhrInitCommands.status == 200) {
        var data = JSON.parse(xhrInitCommands.responseText);
        //Store plugin data to variable, no need to process- already laid out nice
        allPlugins = data;
    } else if (xhrInitCommands.status == 404) {
        //For some reason, failed. Server maybe died
        console.log("Missing stuff");
    } else {
        //IDK WTF went wrong, but something went wrong
        console.log("Unknown error");
    }
}






//-----------------------------Button Controls Settings-------------------
function SetCommandButton() {
    if (selectedButton >= 0) {
        var newButtonCommand = {};
        var plugin = allPlugins.find(element => element.name == pluginSelector.value);
        var command = plugin.commands.find(element => element.name == commandSelector.value);

        newButtonCommand.buttonNum = selectedButton;
        newButtonCommand.callName = plugin.callName + "." + command.callName;
        var valuePairs = document.getElementById("CommandInputs").value.split(/\r\n|\n\r|\n|\r/);
        var sendData = [];

        //If no value set, generate generic
        if (valuePairs.length > 0 && valuePairs[0] != '') {
            valuePairs.forEach(element => {
                var newData = {};
                var setData = element.split(':');
                newData.name = setData[0];
                newData.value = setData[1];
                sendData.push(newData);
            });
        } else {
            var newData = {}
            newData.name = newData.value = "Default";
            sendData.push(newData);
        }
        newButtonCommand.sendData = sendData;

        var checkButton = setButtons.find(element => element.buttonNum == selectedButton);
        if (checkButton != null) {
            //Replace old existance with new, lazy delete first 
            setButtons.splice(setButtons.indexOf(checkButton), 1);
        }
        setButtons.push(newButtonCommand);
    }
}


function ChangeCommandSelection() {
    var plugin = allPlugins.find(element => element.name == pluginSelector.value);
    var command = plugin.commands.find(element => element.name == commandSelector.value);

    commandDesc.innerHTML = "<strong>Description:</strong> " + command.description;
}

function OpenButtonControls() {
    if (buttonControlOpen == false) {
        buttonControlOpen = true;
        document.getElementById("ButtonControls").style.display = "inline-block";

        //Remove all prior existing, just in case
        while (pluginSelector.firstChild) {
            pluginSelector.removeChild(pluginSelector.firstChild);
        }
        while (commandSelector.firstChild) {
            commandSelector.removeChild(commandSelector.firstChild);
        }
        //Fill plugins list
        allPlugins.forEach(plugin => {
            var newOption = document.createElement("option");
            newOption.text = newOption.value = plugin.name;
            pluginSelector.appendChild(newOption);
        });
        //Fill plugins for first plugin package
        allPlugins[0].commands.forEach(cmd => {
            var newOption = document.createElement("option");
            newOption.value = newOption.text = cmd.name;
            commandSelector.appendChild(newOption);
        });
        //Set first command details
        commandDesc.innerHTML = "<strong>Description: </strong>" + allPlugins[0].commands[0].description;
    }
}

function CloseButtonControls() {
    buttonControlOpen = false;
    document.getElementById("ButtonControls").style.display = "none";
    //Visually deselect previous button
    if (selectedButton >= 0) {
        document.querySelector("[data-btnID='" + selectedButton + "']").classList.remove("HighlightButton");
    }
    //Remove any selected button
    selectedButton = -1;
}

function PerformCommand(btn) {
    if (buttonControlOpen) {
        //Visually deselect previous button
        if (selectedButton >= 0) {
            document.querySelector("[data-btnID='" + selectedButton + "']").classList.remove("HighlightButton");
        }

        selectedButton = btn.getAttribute("data-btnID");
        btn.classList.add("HighlightButton");
    } else {
        var cmdToPerform = setButtons.find(element => element.buttonNum === btn.getAttribute("data-btnID"));
        if (cmdToPerform === null) {
            //nothing to perform
            return;
        }
        //Create variable to send data properly
        var sendData = {};
        sendData.command = cmdToPerform.callName;
        sendData.values = cmdToPerform.sendData;

        //Perform request

        var xmlCommand = new XMLHttpRequest();
        xmlCommand.open("POST", "/Command");
        xmlCommand.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        xmlCommand.send(JSON.stringify(sendData));
        xmlCommand.onload = () => {
            if (xmlCommand.status == 200) {
                //Worked out well
            } else if (xmlCommand.status == 404) {
                //For some reason, failed. Server maybe died
                console.log("URL not found");
            } else {
                //IDK WTF went wrong, but something went wrong
                console.log("Failed to perform command: " + sendData.command);
            }
        }
    }
}






function FixButtonDataID() {
    var allButtons = document.getElementsByClassName("ButtonController");

    for (var i = 0; i < allButtons.length; i++) {
        allButtons[i].setAttribute("data-btnID", i);
    }
}

function AddButton() {
    var newButton = document.importNode(ButtonTemplate.content, true);
    var btnObj = document.getElementById("Controller").appendChild(newButton);
    allButtons.push(btnObj);
    //Lazy, just do it every time. Is quick anyways, for most most devices
    //Someone else or I will come up with obviously easier & better solution
    FixButtonDataID();
}


//------------------------------Visual Design Settings--------------------
function ChangeButtonCount(val) {
    var changeAmount = val - buttonCount;
    buttonCount = val;

    if (changeAmount == 0) {
        //Nothing changed, leave
        return;
    }

    if (changeAmount > 0) {
        //Adding buttons
        for (var i = 0; i < changeAmount; i++) {
            AddButton();
        }
    } else {
        //Removing buttons
        var allButtons = document.getElementsByClassName("ButtonController");
        for (var i = 0; i < changeAmount * -1; i++) {
            document.getElementById("Controller").removeChild(allButtons[allButtons.length - (1 + i)]);
        }
    }
}

function ChangeGap(val) {
    gap = val;
    r.style.setProperty("--Button-Margin", gap + "px");
    RecalculateWidth();
}

function ChangeColumns(val) {
    columns = val;
    RecalculateWidth();
}

function ChangeRadius(val) {
    cornerRadius = val;
    r.style.setProperty("--Button-Radius", cornerRadius + "px");
    RecalculateWidth();
}

function ChangeBorderWidth(val) {
    borderWidth = val;
    r.style.setProperty("--Button-Border-Width", borderWidth + "px");
    RecalculateWidth();
}

function ChangeButtonSize(val) {
    buttonSize = val;
    r.style.setProperty("--Button-Size", buttonSize + "px");
    RecalculateWidth();
}

function RecalculateWidth() {
    var borders = borderWidth * 2;
    var margins = gap * 2;
    var columnWidth = columns * buttonSize;
    var totalWidth = ((borders + margins) * columns) + columnWidth;

    r.style.setProperty("--Controller-Width", totalWidth + "px");
}


//------------------------UI Display Control----------------------

function CloseSettings() {
    //One cannot close settings when menus are hidden
    if (menusHidden == false) {
        document.getElementById("Settings").style.display = "none";
    }
}

function OpenSettings() {
    document.getElementById("Settings").style.display = "inline-block";
}

function ShowHideMenus() {
    menusHidden = !menusHidden;

    if (menusHidden) {
        //Hide the menus
        document.getElementById("ControllerHeader").style.display = "none";
    } else {
        //Show the menus
        document.getElementById("ControllerHeader").style.display = "inline-block";
    }
}

function ShrinkUnshrinkSettings() {
    settingsShrunk = !settingsShrunk;

    if (settingsShrunk) {
        //Shrink Settings
        document.getElementById("Settings").style.maxHeight = "20px";
        document.getElementById("Settings").style.overflow = "hidden";
        document.getElementById("Settings").style.marginTop = "100px";
    } else {
        //Unshrink Settings
        document.getElementById("Settings").style.removeProperty("max-height");
        document.getElementById("Settings").style.removeProperty("overflow");
        document.getElementById("Settings").style.removeProperty("margin-top");
    }
}



/* Classes */
class Command {
    constructor() {
        //String sent to perform command
        this.command;
        //String of plugin name
        this.plugin;
        //Command Name
        this.name;
        //Description
        this.description;
        //Expected Values
        this.expectedValues;
    }
}