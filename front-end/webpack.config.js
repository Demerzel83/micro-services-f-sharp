const path = require("path")
const backend = "https://localhost:5001/";
const proxy = require("http-proxy-middleware");
module.exports = {
    mode: "none",
    entry: "./src/App.fsproj",
    devServer: {
        port: 5002,
        contentBase: path.join(__dirname, "./dist"),
        proxy: {
            '/api': {
                target: 'https://localhost:5001/',
                pathRewrite: {
                    "^/api": ""
                  },
                  secure: false
            }
          }
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }]
    }
}