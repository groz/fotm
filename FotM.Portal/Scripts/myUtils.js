function asLocalTime(t)
{
    var d = new Date(t + " UTC");
    document.write(d.toLocaleDateString());
    document.write(" ");
    document.write(d.toLocaleTimeString());
}