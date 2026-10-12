export interface ICookiesStorageService {
  readStorage(key: string): any;
  setStorage(key: string, storage: any): any;
  removeStorage(key: string): any;
}
