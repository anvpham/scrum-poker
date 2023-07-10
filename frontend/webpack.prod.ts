import { merge } from 'webpack-merge';
import config from './webpack.common';
import { EnvironmentPlugin } from 'webpack';

export default merge(config, {
  mode: 'production',
  plugins: [
	new EnvironmentPlugin({
		API_URL: 'https://localhost:5001'
	})
  ],
});