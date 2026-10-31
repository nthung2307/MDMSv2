import { atom } from 'recoil';

export interface SplitViewState {
  isOpen: boolean;
  url: string | null;
  title: string;
  widthPercent: number; // e.g. 50, 65, 80
}

export const splitViewState = atom<SplitViewState>({
  key: 'SPLIT_VIEW_STATE',
  default: {
    isOpen: false,
    url: null,
    title: '',
    widthPercent: 50,
  },
});
