module.exports = function(req, res){
    if(!req.query.name){
        res.status(400).json({ error: 'name must not be null.'});
    }
};
