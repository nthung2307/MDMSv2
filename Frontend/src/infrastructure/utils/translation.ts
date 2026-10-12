import Constants from '@/core/application/common/constants';
import LocalStorageService from '@/infrastructure/services/localStorage.service';

export const _t = (key: any, ...params: any[]) => {
  let localStorageService = new LocalStorageService();
  let languageSelected: any = localStorageService.readStorage(Constants.LANGUAGE_SELECTED_STORAGE);

  let languagesData: any = localStorageService.readStorage(Constants.LIST_DATA_TRANSLATE_STORAGE);
  let dataByKey: any =
    languageSelected && languagesData && languagesData[key]
      ? languagesData[key][languageSelected?.value]
      : null;

  // Find the key
  const rawTranslation = dataByKey;
  // Set the translation as the key as default, in case of not found
  let translation = key;
  // Set the translation as the found one
  if (rawTranslation) {
    translation = rawTranslation;
  }
  // parse the prams
  if (params != null && params.length > 0) {
    for (let i = 0; i < params.length; i++) {
      translation = translation.replace(`{${i}}`, params[i]);
    }
  }
  return translation;
};
