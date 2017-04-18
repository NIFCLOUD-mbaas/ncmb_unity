module.exports = function(req, res){
    if(req.headers.key == 'value'){
        res.send(req.headers.key);
    }else {
        res.send('hello');
    }
};
