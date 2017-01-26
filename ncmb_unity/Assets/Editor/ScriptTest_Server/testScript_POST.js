module.exports = function(req, res){
    if(req.body["name"] !== undefined){
        res.send('hello,' + req.body["name"]);
    }else {
        res.send('hello');
    }
};
