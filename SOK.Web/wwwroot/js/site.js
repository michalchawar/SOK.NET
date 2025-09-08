// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
//
/// <reference path="../lib/jquery/dist/jquery.js" />

const notifications = {

    _createNotification: function (type = this._types.default, text = '') {
        let alertElement = parseHTMLString(`
            <div role="alert" class="alert ${type.className} font-bold text-white transition 
                                     duration-500 opacity-0 shadow-md mb-2.5">
                <i class="bi bi-${type.iconName}"></i>
                <span>${text}</span>
            </div>
        `);

        let progressElement = timeLeft.create(this._durationInMs);
        let container = $("#notificationContainer");

        alertElement.append(progressElement.element);
        container.append(alertElement);

        reflow(alertElement[0]);

        alertElement.removeClass("opacity-0");
        progressElement.start(() => {
            alertElement.addClass("-translate-y-100");
            setTimeout(() => alertElement.remove(), 2000);
        });
    },
    _durationInMs: 4000,
    _types: {
        default: {
            className: '',
            iconName: 'info-circle'
        },
        success: {
            className: 'alert-success',
            iconName: 'check-circle'
        },
        error: {
            className: 'alert-error',
            iconName: 'x-circle'
        },
        warning: {
            className: 'alert-warning',
            iconName: 'exclamation-triangle'
        },
        info: {
            className: 'alert-info',
            iconName: 'info-circle'
        },
    },
    success: function (text = '') {
        this._createNotification(this._types.success, text);
    },
    error: function (text = '') {
        this._createNotification(this._types.error, text);
    },
    info: function (text = '') {
        this._createNotification(this._types.info, text);
    },
    warning: function (text = '') {
        this._createNotification(this._types.warning, text);
    },
}

const timeLeft = {
    create: function (timeInMs = 5000) {
        let progressElement = parseHTMLString(`
            <div class="radial-progress after:opacity-0 before:transition-opacity before:duration-200"
                 style="--value:105; --size: 1rem; --thickness: 0.15rem; transition: --radialprogress ${timeInMs / 1000}s linear;" 
                 aria-valuenow="100" role="progressbar"></div>
        `);

        return {
            element: progressElement,
            start: function (callback = function () { }) {
                progressElement.css("--value", "0");
                setTimeout(() => {
                    this.isFinished = true;
                    progressElement.addClass("before:opacity-0");
                    callback();
                }, timeInMs);
            },
            isFinished: false,
        }
    }
}

function parseHTMLString(str = '') {
    return $($.parseHTML(`
        ${str}
    `.trim().replaceAll('\n', '')));
}

function reflow(element) {
    if (element === undefined) {
        element = document.documentElement;
    }
    void (element.offsetHeight);
}
