import dayjs from 'dayjs';
import jwtDecode from 'jwt-decode';

import Constants from '@/core/application/common/constants';
import CookiesStorageService from '@/infrastructure/services/cookiesStorage.service';
import LocalStorageService from '@/infrastructure/services/localStorage.service.ts';
import { _t } from '@/infrastructure/utils/translation';

export const getISODate = async (dateString: string) => {
  const date = new Date(dateString);
  date.setHours(date.getHours() - 7);
  return date.toISOString();
};
export const splitIntoChunks = (data: any[], chunkSize: number) => {
  const result = [];
  for (let i = 0; i < data.length; i += chunkSize) {
    const chunk = data.slice(i, i + chunkSize);
    while (chunk.length < chunkSize) {
      chunk.push(null);
    }
    result.push(chunk);
  }
  return result;
};
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

export const getListUserRoles = () => {
  return Object.values(Constants.Role).map((role: any) => ({
    label: role.key,
    value: role.value,
  }));
};

export function isObjectEmptyOrHasEmptyValues(obj: Record<string, any>): boolean {
  // Check if the object has no key-value pairs
  if (Object.keys(obj).length === 0) {
    return true;
  }

  // Check if any of the object's keys have null, undefined, or empty string values
  for (const key in obj) {
    if (obj[key] === null || obj[key] === '' || obj[key] === undefined) {
      return true;
    }
  }

  return false;
}

export const isValidData = (validate: any): boolean => {
  return !Object.values(validate).some((it: any) => it.isError);
};

// export const getBooleanAccountStatus = (status: string) => {
//   switch (status) {
//     case Constants.AccountStatus.Active:
//       return true;
//     case Constants.AccountStatus.Deactivate:
//       return false;
//     case Constants.AccountStatus.Deleted:
//     default:
//       return false;
//   }
// };

export const getTimeline = (createdTime: any) => {
  const now = dayjs();
  const targetTime = dayjs(createdTime); //

  const diffInSeconds = now.diff(targetTime, 'second');
  const diffInMinutes = now.diff(targetTime, 'minute');
  const diffInHours = now.diff(targetTime, 'hour');
  const diffInDays = now.diff(targetTime, 'day');
  const diffInMonths = now.diff(targetTime, 'month');
  const diffInYears = now.diff(targetTime, 'year');

  switch (true) {
    case diffInSeconds < 60:
      return 'Vài giây trước';

    case diffInMinutes < 60:
      return `${diffInMinutes} phút trước`;

    case diffInHours < 24:
      return `${diffInHours} giờ trước`;
    case diffInDays < 30:
      return `${diffInDays} ngày trước`;
    case diffInMonths < 12:
      return `${diffInMonths} tháng trước`;

    default:
      return `${diffInYears} năm trước`;
  }
};

export const renderTextNumber = (number: any) => {
  if (!number) return '00';
  return Number(number) > 9 ? number : `0${number}`;
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

export const isFormFieldsEmpty = (fields: string[], dataForm: Record<any, any>) => {
  return fields.some((field) => !dataForm[field]);
};

export const deleteParams = (params: any, keysToRemove: string[]) => {
  const updatedParams = { ...params }; // Create a copy of the original params
  keysToRemove.forEach((key) => {
    if (key in updatedParams) {
      // Check if the key exists in the object
      delete updatedParams[key]; // Remove the specified key
    }
  });
  return updatedParams; // Return the updated object
};

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

export const formatNumberWithCommas = (number: number) => {
  return number ? number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',') : 0;
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

export const convertDateTime = (isoString: string, format: string = 'hh:mi dd-mm-yyyy') => {
  if (!isoString) return 'N/A';
  const date = new Date(isoString);

  const map: Record<string, string> = {
    dd: String(date.getDate()).padStart(2, '0'),
    mm: String(date.getMonth() + 1).padStart(2, '0'),
    yyyy: String(date.getFullYear()),
    hh: String(date.getHours()).padStart(2, '0'),
    mi: String(date.getMinutes()).padStart(2, '0'),
    ss: String(date.getSeconds()).padStart(2, '0'),
  };

  return format.replace(/dd|mm|yyyy|hh|mi|ss/g, (match) => map[match]);
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

export const getAccountRole = () => {
  const localStorage = new LocalStorageService();
  const accessToken: string = localStorage.readStorage(Constants.API_TOKEN_STORAGE);
  if (!accessToken) return null;
  const accountRole: any = jwtDecode(accessToken);
  return accountRole?.role;
};

export const getAccountId = () => {
  const localStorage = new LocalStorageService();
  const accessToken: string = localStorage.readStorage(Constants.API_TOKEN_STORAGE);
  const accountID: any = jwtDecode(accessToken);
  return accountID?.id;
};

export const isMatchRoles = (roles: string[]) => {
  const accountRole = getAccountRole();
  return roles.includes(accountRole);
};

export const numberFormatter = new Intl.NumberFormat('en-US', {
  style: 'decimal',
  maximumFractionDigits: 0,
});

// chartjs custom plugin
export const customDashedLinePlugin = {
  id: 'customDashedLinePlugin',
  afterDraw: (chart: any) => {
    const { ctx } = chart;
    if (chart.tooltip && chart.tooltip._active && chart.tooltip._active.length) {
      const tooltipItem = chart.tooltip._active[0];
      const { x, y } = tooltipItem.element;

      ctx.save();
      ctx.setLineDash([5, 5]); // Dashed line style [dash length, gap length]
      ctx.strokeStyle = 'rgba(0, 0, 0, 0.5)'; // Dashed line color
      ctx.lineWidth = 1;

      // Draw vertical line
      ctx.beginPath();
      ctx.moveTo(x, chart.scales.y.top);
      ctx.lineTo(x, chart.scales.y.bottom);
      ctx.stroke();

      // Draw horizontal line
      ctx.beginPath();
      ctx.moveTo(chart.scales.x.left, y);
      ctx.lineTo(chart.scales.x.right, y);
      ctx.stroke();

      ctx.restore();
    }
  },
};

export const rescaleYAxis = {
  id: 'adjustYAxis',
  beforeUpdate(chart: any) {
    const yAxis = chart.scales.y;
    const visibleDatasets = chart.data.datasets.filter((dataset: any) => !dataset.hidden);
    const allValues = visibleDatasets.flatMap((dataset: any) => dataset.data);

    if (allValues.length) {
      const maxValue = Math.max(...allValues);
      yAxis.options.max = Math.ceil(maxValue * 1.2); // Set Y-axis max with some padding
    } else {
      yAxis.options.max = 1; // Default when no datasets are visible
    }
  },
};

export const fakeLinePlugin = {
  id: 'fakeLinePlugin',
  afterDraw: (chart: any) => {
    const ctx = chart.ctx;
    const xAxis = chart.scales.x;

    // Draw line from 0:00 (first point) to 23:00 (last point) for the "current" dataset
    const currentDatasetMeta = chart.getDatasetMeta(0); // Index 0 is the "current" dataset
    if (currentDatasetMeta) {
      const firstPoint = currentDatasetMeta.data[0]; // First data point
      const lastPoint = currentDatasetMeta.data[currentDatasetMeta.data.length - 1]; // Last data point

      if (firstPoint && lastPoint) {
        ctx.save();
        ctx.beginPath();
        ctx.setLineDash([5, 5]); // Optional dashed line
        ctx.strokeStyle = 'blue'; // Line color
        ctx.lineWidth = 2; // Line thickness
        ctx.moveTo(firstPoint.x, firstPoint.y);
        ctx.lineTo(xAxis.left, firstPoint.y); // Draw to start of X-axis
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(lastPoint.x, lastPoint.y);
        ctx.lineTo(xAxis.right, lastPoint.y); // Draw to end of X-axis
        ctx.stroke();
        ctx.restore();
      }
    }

    // Repeat for "previous" dataset
    const previousDatasetMeta = chart.getDatasetMeta(1); // Index 1 is the "previous" dataset
    if (previousDatasetMeta) {
      const firstPoint = previousDatasetMeta.data[0];
      const lastPoint = previousDatasetMeta.data[previousDatasetMeta.data.length - 1];

      if (firstPoint && lastPoint) {
        ctx.save();
        ctx.beginPath();
        ctx.setLineDash([5, 5]); // Optional dashed line
        ctx.strokeStyle = 'rgba(102,102,102,0.2)'; // Line color
        ctx.lineWidth = 2; // Line thickness
        ctx.moveTo(firstPoint.x, firstPoint.y);
        ctx.lineTo(xAxis.left, firstPoint.y);
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(lastPoint.x, lastPoint.y);
        ctx.lineTo(xAxis.right, lastPoint.y);
        ctx.stroke();
        ctx.restore();
      }
    }
  },
};

export const adjustColumnGap = {
  id: 'adjustColumnGap',
  beforeDatasetsDraw(chart: any) {
    const ctx = chart.ctx;
    const meta = chart.getDatasetMeta(0); // Get the first dataset (adjust if needed)

    if (meta.type === 'bar') {
      const barIndex = 1; // Specify the column index you want to adjust
      const bar = meta.data[barIndex]; // Get the specific bar element

      if (bar) {
        ctx.save();
        ctx.fillStyle = 'rgba(255, 99, 132, 0.5)'; // Change the color for visibility
        ctx.fillRect(
          bar.x - bar.width / 2 - 10, // Adjust position (move left by 10 pixels)
          bar.y,
          bar.width + 20, // Adjust width (increase by 20 pixels)
          bar.base - bar.y
        );
        ctx.restore();
      }
    }
  },
};
