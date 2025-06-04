declare module 'vite-plugin-optimizer' {
  import { Plugin } from 'vite';
  function optimizer(options: any): Plugin;
  export default optimizer;
}
