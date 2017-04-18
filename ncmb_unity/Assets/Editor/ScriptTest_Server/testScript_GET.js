module.exports = function(req, res){
    if(Object.keys(req.query).length >= 2){
        return res.send('count:' + Object.keys(req.query).length);
    }

    if(req.query.name){
        res.send('hello,' + req.query.name);
    }else {
        res.send('hello');
    }
};
