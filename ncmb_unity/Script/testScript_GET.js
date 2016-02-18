module.exports = function(req, res){
    if(req.query.name){
        res.send('hello,' + req.query.name);
    }else {
        res.send('hello');
    }
};
