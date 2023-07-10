import { merge } from 'webpack-merge';
import config from './webpack.common';
import { EnvironmentPlugin } from 'webpack';

export default merge(config, {
  mode: 'development',
  devtool: 'inline-source-map',
  devServer: {
    host: 'localhost',
    historyApiFallback: true,
    clientLogLevel: 'none',
    port: 3000,
  },
  plugins: [
	new EnvironmentPlugin({
		API_URL: 'https://localhost:5001'
	})
  ],
});