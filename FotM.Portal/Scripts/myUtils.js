function asLocalTime(t)
{
    var d = new Date(t + " UTC");
    return d.toLocaleDateString() + " " + d.toLocaleTimeString();
}