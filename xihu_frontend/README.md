### 运行

```sh
npm run dev
```
或
```
npm run electron:dev
```

### axios的使用
对于private接口，使用api
```
import api from '../../api/index.js';
```
对于public接口，使用api_nojwt
```
import api_nojwt from '../../api/index_nojwt.js';
```


