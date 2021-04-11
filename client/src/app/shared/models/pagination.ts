import {IProduct} from './product';

export interface IPagination {
    pageIndex: number;
    pagesize: number;
    count: number;
    data: IProduct[];
  }
  
  