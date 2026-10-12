import moment from 'moment';

import Constants from '@/core/application/common/constants';
import { IDateTimeService } from '@/core/application/interfaces/dateTime.interface';

export default class DateTimeService implements IDateTimeService {
  currentDateTime(format: string = Constants.DateTime.DateTimeFormat) {
    return moment(new Date()).format(format);
  }

  formatDateTime(date: string, format: string = Constants.DateTime.DateFormat) {
    return moment(new Date(date)).format(format);
  }
}
