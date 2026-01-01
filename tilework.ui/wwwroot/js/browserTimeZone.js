window.timeZoneInterop = window.timeZoneInterop || {
    getTimeZone: () => Intl.DateTimeFormat().resolvedOptions().timeZone ?? "UTC"
};
