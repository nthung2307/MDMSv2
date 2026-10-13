import { atom } from 'recoil';

export const ErrorDataState = atom({
  key: 'ERROR_DATA', // unique ID (with respect to other atoms/selectors)
  default: false,
});
