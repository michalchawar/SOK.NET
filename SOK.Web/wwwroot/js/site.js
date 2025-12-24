// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
//
/// <reference path="../lib/jquery/dist/jquery.js" />

// Disable console in production
// Sprawdza czy document.documentElement ma atrybut data-env="Development"
(function() {
    // Sprawdź środowisko z atrybutu HTML (zostanie ustawione w _Layout.cshtml)
    const isProduction = document.documentElement.getAttribute('data-env') !== 'Development';
    
    if (isProduction) {
        // Zachowaj console.error dla krytycznych błędów (opcjonalnie)
        const originalError = console.error.bind(console);
        
        // Nadpisz wszystkie metody console (oprócz error)
        console.log = function() {};
        console.debug = function() {};
        console.info = function() {};
        console.warn = function() {};
        
        // Opcjonalnie: wyłącz też console.error (odkomentuj jeśli chcesz)
        // console.error = function() {};
    }
})();

const notifications = {

    _createNotification: function (type = this._types.default, text = '') {
        let alertElement = parseHTMLString(`
            <div role="alert" class="text-white transition 
                    duration-500 opacity-0 shadow-md mb-2.5 flex items-center">
                <div class="bg-${type.className} rounded-sm p-4 text-2xl">   
                    <i class="bi bi-${type.iconName}"></i>
                </div>
                <div class="notification-content rounded-xs bg-${type.className}/60 p-3 flex-1 flex items-center">
                    <div class="flex-1">${text}</div>
                </div>
            </div>
        `);

        let progressElement = timeLeft.create(this._durationInMs);
        let container = $("#notification-container");

        alertElement.children(".notification-content").append(progressElement.element);
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
            // bg-success bg-success/60
            className: 'success',
            iconName: 'check-circle'
        },
        error: {
            // bg-error bg-error/60
            className: 'error',
            iconName: 'x-circle'
        },
        warning: {
            // bg-warning bg-warning/60
            className: 'warning',
            iconName: 'exclamation-triangle'
        },
        info: {
            // bg-info bg-info/60
            className: 'info',
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
    resolveType: function (typeString = '', text = '') {
        switch (typeString.toLowerCase()) {
            case 'success': this.success(text); break;
            case 'error': this.error(text); break;
            case 'info': this.info(text); break;
            case 'warning': this.warning(text); break;
            default: return;
        }
    }
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


window.getContrastColor = function(hexColor) {
    if (!hexColor) return '#000000';

    // Remove the hash symbol if present
    hexColor = hexColor.replace('#', '');

    // Convert hex to RGB
    const r = parseInt(hexColor.substr(0, 2), 16);
    const g = parseInt(hexColor.substr(2, 2), 16);
    const b = parseInt(hexColor.substr(4, 2), 16);
    
    // Calculate the luminance
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    
    // Return black for light colors and white for dark colors
    return luminance > 0.5 ? '#18181b' : '#ecf9ff';
}

window.declinateWord = function(count, singular, pluralFew, pluralMany) {
    const mod10 = count % 10;
    const mod100 = count % 100;

    if (mod10 === 1 && mod100 !== 11) {
        return singular;
    } else if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) {
        return pluralFew;
    } else {
        return pluralMany;
    }
}

function registerVueApp(appElementId) {
    let appElement = $(`#${appElementId}`);
    appElement.attr("data-is-loaded", "true");
}

function initThemeToggle() {
    const toggle = $("#theme-toggle");
    const label = $("#theme-label");
    const currentTheme = document.documentElement.getAttribute("data-theme");
    
    // Set initial state
    if (currentTheme === "dark") {
        toggle.prop("checked", true);
        label.text("Tryb jasny");
    } else {
        toggle.prop("checked", false);
        label.text("Tryb ciemny");
    }
    
    // Handle toggle change
    toggle.parent().parent().on("click", function() {
        toggle.prop("checked", !toggle.prop("checked"));
        const newTheme = toggle.prop("checked") ? "dark" : "light";
        document.documentElement.setAttribute("data-theme", newTheme);
        localStorage.theme = newTheme;
        label.text(toggle.prop("checked") ? "Tryb jasny" : "Tryb ciemny");
    });
}

$(function() {
    initThemeToggle();
});