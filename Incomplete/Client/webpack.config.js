var path = require("path");

module.exports = {
    mode: "development",
    entry: "./Client.fsproj",
    output: {
        path: path.join(__dirname, "./public"),
        filename: "bundle.js",
    },
    devServer: {
        publicPath: "/",
        contentBase: "./public",
        port: 8085,
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}