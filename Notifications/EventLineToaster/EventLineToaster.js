// Keep these lines for a best effort IntelliSense of Visual Studio 2017 and higher.
/// <reference path="./../../Packages/Beckhoff.TwinCAT.HMI.Framework.12.760.48/runtimes/native1.12-tchmi/TcHmi.d.ts" />

/*
 * Generated 10/19/2023 2:22:55 PM
 * Copyright (C) 2023
 */
var TcHmi;
(function (/** @type {globalThis.TcHmi} */ TcHmi) {
    let Controls;
    (function (/** @type {globalThis.TcHmi.Controls} */ Controls) {
        let Notifications;
        (function (Notifications) {
            class EventLineToaster extends TcHmi.Controls.Beckhoff.TcHmiEventLine {

                /*
                Attribute philosophy
                --------------------
                - Local variables are not set in the class definition, so they have the value 'undefined'.
                - During compilation, the Framework sets the value that is specified in the HTML or in the theme (possibly 'null') via normal setters.
                - Because of the "changed detection" in the setter, the value is only processed once during compilation.
                - Attention: If we have a Server Binding on an Attribute, the setter will be called once with null to initialize and later with the correct value.
                */

                /**
                 * Constructor of the control
                 * @param {JQuery} element Element from HTML (internal, do not use)
                 * @param {JQuery} pcElement precompiled Element (internal, do not use)
                 * @param {TcHmi.Controls.ControlAttributeList} attrs Attributes defined in HTML in a special format (internal, do not use)
                 * @returns {void}
                 */
                constructor(element, pcElement, attrs) {
                    /** Call base class constructor */
                    super(element, pcElement, attrs);

                    this.__enableNotifications = true;
                    this.__notifier = {};
                }
                /**
                 * Raised after the control was added to the control cache and the constructors of all base classes were called.
                 */
                __previnit() {
                    // Fetch template root element
                    this.__elementTemplateRoot = this.__element.find('.TcHmi_Controls_Notifications_EventLineToaster-Template');
                    if (this.__elementTemplateRoot.length === 0) {
                        throw new Error('Invalid Template.html');
                    }
                    // Call __previnit of base class
                    super.__previnit();
                }
                /**
                 * Is called during control initialization after the attribute setters have been called. 
                 * @returns {void}
                 */
                __init() {
                    super.__init();
                }
                /**
                 * Is called by the system after the control instance is inserted into the active DOM.
                 * Is only allowed to be called from the framework itself!
                 */
                __attach() {
                    super.__attach();
                    /**
                     * Initialize everything which is only available while the control is part of the active dom.
                     */

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
                /**
                 * Is called by the system when the control instance is no longer part of the active DOM.
                 * Is only allowed to be called from the framework itself!
                 */
                __detach() {
                    super.__detach();

                    /**
                     * Disable everything that is not needed while the control is not part of the active DOM.
                     * For example, there is usually no need to listen for events!
                     */
                }
                /**
                 * Destroy the current control instance.
                 * Will be called automatically if the framework destroys the control instance!
                 */
                destroy() {
                    /**
                     * Ignore while __keepAlive is set to true.
                     */
                    if (this.__keepAlive) {
                        return;
                    }
                    super.destroy();
                    /**
                     * Free resources like child controls etc.
                     */
                }

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

                getEnableNotifications() {
                    return this.__enableNotifications;
                }

                setEnableNotifications(value) {
                    this.__enableNotifications = value || false;
                }

                notify(type, text) {
                    this.__notifier.open({
                        type: type,
                        message: text
                    });
                }
            }
            Notifications.EventLineToaster = EventLineToaster;
        })(Notifications = Controls.Notifications || (Controls.Notifications = {}));
    })(Controls = TcHmi.Controls || (TcHmi.Controls = {}));
})(TcHmi || (TcHmi = {}));

/**
 * Register Control
 */
TcHmi.Controls.registerEx('EventLineToaster', 'TcHmi.Controls.Notifications', TcHmi.Controls.Notifications.EventLineToaster);
