import { ToastContent, toast } from 'react-toastify';

import Constants from '@/core/application/common/constants';

/**
 * Helper để trích xuất message dù gọi với 1 tham số (message) hay 2 tham số (_translator, message)
 */
const extractMessage = (param1: any, param2?: ToastContent, defaultText = ''): ToastContent => {
  if (param2 !== undefined && param2 !== null && param2 !== '') {
    return param2;
  }
  if (param1 !== undefined && param1 !== null && param1 !== '') {
    return param1;
  }
  return defaultText;
};

export const notifyInfo = (param1: any, param2?: ToastContent) => {
  const message = extractMessage(param1, param2, 'Thông tin');
  toast.info(message, {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifySuccess = (param1: any, param2?: ToastContent) => {
  const message = extractMessage(param1, param2, 'Thành công');
  toast.success(message, {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifyError = (param1: any, param2?: ToastContent) => {
  const message = extractMessage(param1, param2, 'Có lỗi xảy ra');
  toast.error(message, {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifyWarning = (param1: any, param2?: ToastContent) => {
  const message = extractMessage(param1, param2, 'Cảnh báo');
  toast.warning(message, {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};
