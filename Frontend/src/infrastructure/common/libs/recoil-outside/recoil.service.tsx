/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/ban-ts-comment */
/* eslint-disable no-async-promise-executor */
/* eslint-disable react-refresh/only-export-components */
import { useEffect } from 'react';
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
    const subscription = finishSetRecoil.subscribe({
      next: (value) => {
        // @ts-ignore
        if (recoilState === value.recoilObj) {
          subscription.unsubscribe();
          setTimeout(() => resolve(recoilState), 10);
        }
      },
    });
    setRecoil.next({ recoilObj: recoilState, value: stateValue });
  });
};
/**
 * Get recoil state value of the given name
 * @param recoilState Name of recoil state
 * @returns Value object
 */
export const getRecoilStateAsync = (recoilState: any) => {
  return new Promise(async (resolve, _reject) => {
    const subscription = returnRecoil.subscribe({
      next: (value) => {
        // @ts-ignore
        if (recoilState === value.recoilObj) {
          subscription.unsubscribe();
          // @ts-ignore
          resolve(value.value);
        }
      },
    });
    getRecoil.next(recoilState);
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

  useEffect(() => {
    const setSub = setRecoil.subscribe({
      next: (value) => {
        setStore(value);
      },
    });

    const getSub = getRecoil.subscribe({
      next: (recoilObj) => {
        getStore(recoilObj);
      },
    });

    return () => {
      setSub.unsubscribe();
      getSub.unsubscribe();
    };
  }, [setStore, getStore]);

  return null;
}
