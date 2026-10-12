/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/ban-ts-comment */
/* eslint-disable no-async-promise-executor */
/* eslint-disable react-refresh/only-export-components */
import { useRecoilCallback } from 'recoil';
import { Subject } from 'rxjs';

const setRecoil = new Subject();
const finishSetRecoil = new Subject();

const getRecoil = new Subject();
const returnRecoil = new Subject();
/**
 * Set recoil state into the given name
 * @param recoilState Name of recoil state
 * @param stateValue Value to be set to the given recoilState
 * @returns
 */
export const setRecoilStateAsync = (recoilState: any, stateValue: any) => {
  return new Promise(async (resolve, _reject) => {
    setRecoil.next({ recoilObj: recoilState, value: stateValue });

    finishSetRecoil.subscribe({
      next: (value) => {
        // @ts-ignore
        if (recoilState === value.recoilObj) {
          setTimeout(() => resolve(recoilState), 10);
        }
      },
    });
  });
};
/**
 * Get recoil state value of the given name
 * @param recoilState Name of recoil state
 * @returns Value object
 */
export const getRecoilStateAsync = (recoilState: any) => {
  return new Promise(async (resolve, _reject) => {
    getRecoil.next(recoilState);
    returnRecoil.subscribe({
      next: (value) => {
        // @ts-ignore
        if (recoilState === value.recoilObj) {
          // @ts-ignore
          resolve(value.value);
        }
      },
    });
  });
};

export default function RecoilOutsideComponent() {
  const setStore = useRecoilCallback(
    ({ set }) =>
      async (n) => {
        // @ts-ignore
        await set(n.recoilObj, () => n.value);
        // @ts-ignore
        finishSetRecoil.next({ recoilObj: n.recoilObj });
      },
    []
  );

  const getStore = useRecoilCallback(
    ({ snapshot }) =>
      async (recoilObj) => {
        // @ts-ignore
        const valueRecoilObj = await snapshot.getPromise(recoilObj);
        returnRecoil.next({ recoilObj: recoilObj, value: valueRecoilObj });
      },
    []
  );

  setRecoil.subscribe({
    next: (value) => {
      setStore(value);
    },
  });

  getRecoil.subscribe({
    next: (recoilObj) => {
      getStore(recoilObj);
    },
  });

  return null;
}
