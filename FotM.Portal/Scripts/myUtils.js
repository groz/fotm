function asLocalTime(t)
{
    var d = new Date(t + " UTC");
    return d.toLocaleDateString() + " " + d.toLocaleTimeString();
}

// http://stackoverflow.com/a/2117523/283975
function genGuid() {
    var result = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
    return result;
}