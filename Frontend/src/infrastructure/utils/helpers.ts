import dayjs from 'dayjs';

import Constants from '@/core/application/common/constants';
import CookiesStorageService from '@/infrastructure/services/cookiesStorage.service';
import { _t } from '@/infrastructure/utils/translation';

export const refactorFormDataCommon = (data: any) => {
  let result: any = {};
  if (Object.keys(data)?.length) {
    Object.keys(data).forEach((key) => {
      if (typeof data[key] === 'string') {
        result = {
          ...result,
          [key]: data[key]?.trim() || '',
        };
      } else {
        result = {
          ...result,
          [key]: data[key],
        };
      }
    });
  }
  return result;
};

export const getListRole = () => {
  const cookiesStorageService = new CookiesStorageService();

  let role = cookiesStorageService.readStorage(Constants.API_ROLE_STORAGE);
  return role != null ? [role.toString()] : [];
};

export type RulesReg = {
  pattern: any;
  message?: string;
};

export function validateReg(value: any, rules: Array<RulesReg>) {
  for (let i = 0; i < rules.length; ++i) {
    if (rules[i].pattern !== undefined && !rules[i].pattern.test(value)) {
      return {
        error: true,
        message: rules[i].message || '',
      };
    }
  }
  return {
    error: false,
    message: '',
  };
}

//common function validate
export const validateFields = (props: {
  isImplicitChange: boolean;
  label: string;
  key: string;
  value: any;
  isRequired?: boolean;
  setError: any;
  error: any;
  rules?: Array<RulesReg>;
}) => {
  let message = {
    error: false,
    message: '',
  };

  if (props.isRequired) {
    if (props.value == null || !props.value?.length) {
      message = {
        error: true,
        message: _t('Vui lòng điền vào mục này'), //`Please enter ${props.label}`
        // message: _t('Vui lòng điền {0}', props.label.toLowerCase()), //`Please enter ${props.label}`
      };
    }
    if (props.value) {
      message = {
        error: false,
        message: ``,
      };
    }
  }

  if (props.rules && props.rules.length && props.value != null && props.value?.toString()?.length) {
    message = validateReg(props.value, props.rules);
  }

  if (props.isImplicitChange) {
    props.error[props.key] = {
      isError: message.error,
      message: message.message,
    };
    return;
  }

  props.setError({
    ...props.error,
    [props.key]: {
      isError: message.error,
      message: message.message,
    },
  });
};

const has = Object.prototype.hasOwnProperty;
export const isEmpty = (prop: any) => {
  return (
    prop === null ||
    prop === undefined ||
    (has.call(prop, 'length') && prop.length === 0) ||
    (prop.constructor === Object && Object.keys(prop).length === 0)
  );
};

export const onScrollToParentOfMessageError = () => {
  // Get all elements with the class name 'message-error'
  const errorElements = document.getElementsByClassName('message-error');

  // Convert HTMLCollection to array for easier manipulation
  const errorElementsArray = Array.from(errorElements);

  // Find the first element with a child element with class 'message'
  const parentOfMessageError = errorElementsArray.find((errorElement) =>
    errorElement.querySelector('.message')
  );

  // Scroll to the parent element of the first message-error element with a child element with class 'message'
  if (parentOfMessageError) {
    parentOfMessageError.parentElement?.scrollIntoView({ behavior: 'smooth' });
  }
};

export const disableDateFuture = (current: any) => {
  return current && current > dayjs().endOf('day');
};

export const generateRandomString = (length: number) => {
  const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  const charactersLength = characters.length;
  for (let i = 0; i < length; i++) {
    result += characters.charAt(Math.floor(Math.random() * charactersLength));
  }
  return result;
};
