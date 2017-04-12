var express = require('express');
var bodyParser = require('body-parser');

var app = express();

app.use(bodyParser.json());

var moduleForGetMethod = require('./testScript_GET.js');
var moduleForPostMethod = require('./testScript_POST.js');
var moduleForPutMethod = require('./testScript_PUT.js');
var moduleForDeleteMethod = require('./testScript_DELETE.js');
var moduleForError = require('./testScript_Error.js');
var moduleForHeader = require('./testScript_Header.js');
var moduleForObjectGetMethod = require('./testScriptObject_GET.js');

var apiVersion = "/2015-09-01";
var servicePath = "/script";

app.get(apiVersion + servicePath + '/testScript_GET.js', moduleForGetMethod);
app.post(apiVersion + servicePath + '/testScript_POST.js', moduleForPostMethod);
app.put(apiVersion + servicePath + '/testScript_PUT.js', moduleForPutMethod);
app.delete(apiVersion + servicePath + '/testScript_DELETE.js', moduleForDeleteMethod);
app.get(apiVersion + servicePath + '/testScript_Error.js', moduleForError);
app.post(apiVersion + servicePath + '/testScript_Header.js', moduleForHeader);
app.get(apiVersion + servicePath + '/testScriptObject_GET.js', moduleForObjectGetMethod);

app.listen(3000, function () {
    console.log('app listening on port 3000');
});
