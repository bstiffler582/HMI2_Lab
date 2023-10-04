## Lab 2: Framework Control

### Contents
1. [Getting Started](#getting-started)
2. [References](#references)
3. [Control Setup](#control-setup)
4. [Scripting](#scripting)
5. [Extending & Debugging](#console-log)
6. [Types & Functions](#functions)

---

### Introduction

The TcHmiEventLine control is a great addition to the framework. It is typically placed outside of the main region, so that the latest alarms and messages can be viewed without navigating away from the current page. The criticism might be that this control does not graphically present the information in a manner that sufficiently grabs the attention of the operator. To improve the control, perhaps we can introduce a better way to notify the user.

In this lab, we will create a Framework Control that extends the TcHmiEventLine and adds 'Toast' notifications for any new event that is raised. We can piggy-back off the existing control functionality to capture the events.

Again, we will be outsourcing the hard part. There are dozens of good JavaScript Toast libraries out there. We will use one called [Notyf](https://github.com/caroso1222/notyf).

<a id="getting-started"></a>

### 1. Getting Started

- We will use the same solution from the first lab
- Verify that the PLC program is still running
- Create a new Framework project
  - Project type = *TwinCAT HMI Framework Project Empty*
  - Let's call our project '**Notifications**'
- Within the *Notifications* project, Add a new item
  - Type = Framework Control (JavaScript)
  - Name the control '**EventLineToaster**'

> Note: The Framework project can contain a number of framework controls.

<a id="references"></a>

### 2. References

There are a couple different references to add here:

**The Beckhoff.TwinCAT.HMI.Controls Nuget package**

Since we are using an existing control as the base of our fancy new Framework Control, we will need to include this package. Add the package as a reference via Nuget. Check the *Manifest.json* file to see if the reference has been added there as well. If not, add it manually:
```json
{
  "modules": [
    {
      "type": "Package",
      "nugetId": "Beckhoff.TwinCAT.HMI.Framework"
    },
    {
      "type": "Package",
      "nugetId": "Beckhoff.TwinCAT.HMI.Controls"
    },
    //...
  ]
}
```

If we were to create our control using TypeScript (it's JavaScript, with more steps!), we would also want to import the type definitions of the control we are extending into our TS configuration. This is accomplished by adding it to the 'include' list within the *tsconfig.tpl.json* file:
```json
{
//...
"include": [
    "$(Beckhoff.TwinCAT.HMI.Framework).InstallPath/TcHmi.d.ts",
    "$(Beckhoff.TwinCAT.HMI.Controls).InstallPath/TcHmiEventLine/TcHmiEventLine.d.ts"
  ]
}
```

**The Notyf JavaScript library**

I have already pulled the necessary files for referencing this library, and have included them in the repo in the [Libs](../Libs/) directory.

Create a new folder called '**Lib**' in the Framework project root (*not* in the control project directory), and add the notyf css, js and license files via right-click -> 'Add Existing Item'.

Finally, make sure our new control knows where to find that notyf library. We will point to it in the *Description.json* file, under `dependencyFiles`:
```json
  "dependencyFiles": [
    //...
    {
      "name": "../Lib/notyf.min.js",
      "type": "JavaScript"
    },
    {
      "name": "../Lib/notyf.min.css",
      "type": "Stylesheet"
    }
  ]
```

<a id="control-setup"></a>

### 3. Control Setup

While we are here in the *Description.json* file, there is more work to be done. This is the file that the TcHmi framework and designer will use to digest our control.

First, remember that we are extending a control from the Beckhoff.TwinCAT.HMI.Controls namespace. All controls inherit from `TcHmi.Controls.System.TcHmiControl`. However, we will be using `TcHmi.Controls.Beckhoff.TcHmiEventLine` as our base control, so we need to change this line:
```json
"base": "TcHmi.Controls.System.TcHmiControl",
```
to this:
```json
"base": "TcHmi.Controls.Beckhoff.TcHmiEventLine",
```

We can also change the default dimensions to match those of our base control:
```json
"properties": {
    //...
    "geometry": {
      "width": 560,
      "height": 40
    }
  }
```

Where did we get that information?

- Build HMI project
- Navigate (in Explorer) to: *\HMI\bin\Beckhoff.TwinCAT.HMI.Controls\TcHmiEventLine*
- Open *Description.json* file

Leave this directory open as we will continue to reference it throughout the lab.

Next, we will add an attribute to our own *Description.json* file. This will allow us to enable/disable our soon to be implemented notification functionality:
```json
"attributes": [
    {
      "displayName": "Enable Notifications",
      "name": "data-tchmi-enable-notifications",
      "propertyGetterName": "getEnableNotifications",
      "propertySetterName": "setEnableNotifications",
      "propertyName": "enableNotifications",
      "type": "tchmi:general#/definitions/Boolean",
      "bindable": true,
      "visible": true,
      "defaultValue": true,
      "defaultValueInternal": true,
      "description": "Enable toast notifications.",
      "category": "Common"
    }
  ]
```

Next, we need to tell the browser how to render our control. Typically this would be custom HTML and CSS, but again since we are extending an existing control (and not modifying it visually), we will just use the markup from our base control.

Copy the markup from the *TcHmiEventLine\Template.html*, and paste it **inside the root element** in the Template.html file of our *EventLineToaster* control:
```html
<div class="TcHmi_Controls_Notifications_EventLineToaster-Template tchmi-box">
    <div class="TcHmi_Controls_Beckhoff_TcHmiEventLine-Template tchmi-box">
        <div class="TcHmi_Controls_Beckhoff_TcHmiEventLine-Icon"></div>
        <div class="TcHmi_Controls_Beckhoff_TcHmiEventLine-Message-Container">
            <span class="TcHmi_Controls_Beckhoff_TcHmiEventLine-Message"></span>
        </div>
        <div class="TcHmi_Controls_Beckhoff_TcHmiEventLine-Button tchmi-box"></div>
    </div>
</div>
```

> Note: The base control's attributes (from the description file), scripting, and stylesheets are inherited when we extend it with our own control. The markup is the only aspect that needs to be duplicated by hand.

Finally, before we can drop our control into the designer, we need to change one little line in the script file. In *EventLineToaster.js* change this:
```js
class EventLineToaster extends TcHmi.Controls.System.TcHmiControl
```
to this:
```js
class EventLineToaster extends TcHmi.Controls.Beckhoff.TcHmiEventLine
```
Once again, telling the framework that we are extending a Beckhoff control instead of starting from scratch.

- Build the 'Notifications' framework project
- Add 'Notifications' as a reference to our HMI project
- Open or reload Desktop.view
- Find and drag the EventLineToaster control onto the page

Note that the control looks and behaves identically to the TcHmiEventLine, with the addition of our added attribute (property).

<a id="scripting"></a>

### 4. Scripting

With **all** of that out of the way, we can now start to implement our Toast notifications.

First, we will define the getter/setter functions for the new 'enable' attribute we have added.

Create a field to store the property value. This will go in the control's constructor method, and we will initialize it to the default value we defined in the description file. While we are in there, we might as well add a field for our notyf object:
```js
constructor(element, pcElement, attrs) {
    //...
    this.__enableNotifications = true;
    this.__notifier = {};
}
```

Next, create the simple getter and setter functions. These will go toward the bottom of the control code, underneath the `destroy()` function:
```js
getEnableNotifications() {
  return this.__enableNotifications;
}

setEnableNotifications(value) {
  this.__enableNotifications = value || false;
}
```

If we rebuild and test in live view again, we should no longer be getting any exceptions.

Now that we have defined the notifier object, let's initialize it with a call across to our third-party library. We can reference the library [repo](https://github.com/caroso1222/notyf) again for implementation details. Properties are fairly self-explanatory, so feel free to tweak them to your liking.

In the `__attach()` function:
```js
__attach() {
  //...
  this.__notifier = new Notyf({
    duration: 3000,
    position: {
      x: 'left',
      y: 'bottom',
    },
    types: [
      {
        type: 'info',
        background: 'mediumslateblue',
        dismissible: true
      },
      {
        type: 'error',
        background: 'indianred',
        dismissible: true
      }
    ]
  });
}
```

Ok, we are ready to raise some Toast notifications... but how?

<a id="console-log"></a>

### 5. Extending & Debugging

Why did we extend the *TcHmiEventLine* control in the first place again? So we can 'hijack' EventLogger messages as they come in. The base control already listens for and enacts on these events.

Take a look at the base control's TypeScript definition file, *TcHmiEventLine.d.ts*. Do we see any properties or methods that may be useful for what we are trying to do? I see a method called `__updateEventLine()`, and a property called `__event`.

We can attempt to override the `__updateEventLine()` method in our control by simply adding a method with the same name in our *EventLineToaster.js* file. To retain the method's functionality, we will call it with `super` in our override. We can also log the `__event` property to the console to see what sort of information is available:
```js
__updateEventLine() {
  super.__updateEventLine();
  console.log(this.__event);
}
```

This call can go right above our property getter/setter functions. You should be able to examine the results of this call by simply re-launching (or refreshing) a live view window.

Luckily, the method is called with every new event, and the `__event` property contains everything we need for our Toast notifications! We can use the object we have already initialized within this same method to open a Toast message each call:
```js
__updateEventLine() {

  super.__updateEventLine();

  if (!this.__enableNotifications)
    return;

  const e = this.__event;

  if (e && e.alarmState == 0) {
    this.__notifier.open({
        type: (e.severity >= 3) ? 'error' : 'info',
        message: e.text
    });
  }
}
```

Refresh and test our new *EventLineToaster*! We now have lovely graphical notifications for every new message or Alarm that is raised in this application.

<a id="functions"></a>

### 6. Types & Functions

We can make raising ad-hoc Toast notifications a breeze as well by defining a function in our control. 

First, in order to make the designer experience better and the function less prone to errors, we can define a custom type in our framework control's *Schema\Types.Schema.json* file:
```json
"definitions": {
  //...
  "ToastType": {
    "type": "string",
    "enum": [
      "info",
      "error"
    ]
  }
}
```

Next, we will add a new function to our *Description.json* file. One of the functions parameters references the new type we've just created:
```json
//...
"functions": [
  {
    "category": "Common",
    "displayName": "Notify",
    "heritable": true,
    "name": "notify",
    "type": null,
    "visible": true,
    "description": "Invoke toast notification.",
    "params": [
      {
        "displayName": "Type",
        "name": "type",
        "type": "tchmi:framework#/definitions/ToastType",
        "visible": true,
        "bindable": false,
        "description": "Type of toast message"
      },
      {
        "displayName": "Text",
        "name": "text",
        "type": "tchmi:general#/definitions/String",
        "visible": true,
        "bindable": false,
        "description": "Toast message text."
      }
    ]
  }
]
//...
```

Lastly, we just need to implement the function on the JavaScript side:
```js
notify(type, text) {
  this.__notifier.open({
    type: type,
    message: text
  });
}
```

Now we can call the function from anywhere that has access to our control. Try adding a call to this function via the Actions & Events editor - e.g. onPressed of the 'Manual Trigger' button.

- How can we make this control even better, more robust, etc.?
- Knowing what's possible (basically anything), do you have any ideas for useful controls?