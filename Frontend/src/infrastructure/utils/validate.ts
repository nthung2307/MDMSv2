import Constants from '@/core/application/common/constants';
import { RulesReg } from '@/infrastructure/utils/helpers';

export function validateEmail(email: string) {
  const reg = /^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$/;
  // return url != null && reg.test(url);
  return email && reg.test(email);
}

export function validateInputPassword(val: string, oldVal = '') {
  if (oldVal && val === oldVal) {
    return false;
  }
  const reg = /(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9])(?=.{8,})/;
  // /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;
  return val && reg.test(val);
}

export function validateName(val: string) {
  const reg = /[a-zA-Z0-9]{3,}$/;
  return val && reg.test(val);
}

export function validateOnlyNameOrNumber(val: string) {
  const reg = /[a-zA-Z0-9]$/;
  return val && reg.test(val);
}

// export function validateCompanyName(val) {
//     let reg = /^[a-zA-Z]+$/;
//     return val && reg.test(val);
// }

export function validateTax(val: string) {
  const reg = /^([0-9-]{10,13})$/;
  return val && reg.test(val);
}

export function validateId(val: string) {
  const reg = /^([0-9])/;
  return val && reg.test(val);
}

export const validateLengthForInput = (maxLength: number, rules?: Array<RulesReg>) => {
  return [
    ...(rules ?? []),
    {
      pattern: new RegExp(`^[\\s\\S]{0,${maxLength}}$`),
      message: `Giới hạn nhập ${maxLength} ký tự`,
    },
  ];
};

export function validatePhoneNumber(val: string) {
  if (!val || typeof val !== 'string') return false;
  return Constants.PATTERN_REGEX.PhoneNumber.test(val.trim());
}

export function validateLandingPhoneNumber(val: string) {
  if (!val || typeof val !== 'string') return false;
  return Constants.PATTERN_REGEX.PhoneNumber.test(val.trim());
}

export function validateImg(img: string) {
  const reg = /\/(jpe?g|png)$/i;
  // return url != null && reg.test(url);
  return img && reg.test(img);
}

export function validateVideo(video: string) {
  const reg = /\/(mpe?g|mp4)$/i;
  // return url != null && reg.test(url);
  return video && reg.test(video);
}

export function validateFileExcel(file: string) {
  const reg = /.*\.(xlsx|xls)/g;
  // return url != null && reg.test(url);
  return file && reg.test(file);
}

export function validateFormProduct(val: string) {
  const reg = /[a-zA-Z0-9]/;
  return val && reg.test(val);
}

export function validateFormInputNumber(val: string) {
  const reg = /^[0-9]\d*(\.\d+)?$/;
  return val && reg.test(val);
}

export function validateFormInputCoefficient(val: string) {
  const reg = /^[0-9]\d*(\.\d+)?$/;
  return val && reg.test(val);
}

export function validateFormInputDiscount(val: string) {
  const reg = /^[0-9]\d*(\.\d+)?$/;
  return val && reg.test(val);
}

export function validateForm(val: string) {
  const reg = /[a-zA-Z0-9]/;
  return val && reg.test(val);
}

export function validateCMND(val: string) {
  const pattern = /^[0-9]{12}$/;
  return val && pattern.test(val);
}
