import { ToastContent, toast } from 'react-toastify';

import Constants from '@/core/application/common/constants';
/**
 * Show toast message, if you leave the message as null or empty, it will show title only
 * @param _translator
 * @param title
 * @param message
 */
export const notifyInfo = (_translator: any, message?: ToastContent) => {
  toast.warning(message || 'Info', {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifySuccess = (_translator: any, message?: ToastContent) => {
  toast.success(message || 'Success', {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifyError = (_translator: any, message?: ToastContent) => {
  toast.error(message || 'Error', {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};

export const notifyWarning = (_translator: any, message?: ToastContent) => {
  toast.warning(message || 'Warning', {
    position: Constants.ToastMessage.Notification.Position,
    autoClose: Constants.ToastMessage.Notification.Duration,
    hideProgressBar: false,
    closeOnClick: false,
    pauseOnHover: true,
    draggable: true,
    theme: 'light',
  });
};
