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
  // let reg = /^(84|0[3|5|7|8|9])+([0-9]{7,8})\b/;
  const reg =
    /^(1900|1800)[0-9]{4}$|(05|03|04|07|08|09|024|028)[0-9]{8}$|(\+84)[0-9]{9}$|(84)[0-9]{9}$|(\+84)[0-9]{8}$|(\+84)[0-9]{10}$|(021[012345689]|023[23456789]|020[3456789]|022[0123456789]|029[01234679]|025[123456789]|026[01239]|027[01234567]|037[01234567])[0-9]{7}$/;
  return val && reg.test(val);
}

export function validateLandingPhoneNumber(val: string) {
  const reg =
    /^(1900|1800)[0-9]{4}$|(05|03|04|07|08|09|024|028)[0-9]{8}$|(\+84)[0-9]{9}$|(84)[0-9]{9}$|(\+84)[0-9]{8}$|(\+84)[0-9]{10}$|(021[012345689]|023[23456789]|020[3456789]|022[0123456789]|029[01234679]|025[123456789]|026[01239]|027[01234567]|037[01234567])[0-9]{7}$/;
  return val && reg.test(val);
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
