console.log("shared.js");

app.provider("shared", function () {
    var sp = {}
    this.set = function (sharedProperties) { sp = sharedProperties; }
    this.$get = function () { return sp; }
});