export const acceptFile = (file: string, _type: string = '') => {
  let accpt = '';
  let typeFile = _type || '';
  if (file && typeFile === '') {
    typeFile = file.slice(file.lastIndexOf('.') + 1);
  }
  // png, jpeg, jpg, pdf, xlsx, xls, doc, docx, ppt, pptx, zip, rar and max file size is 5Mb per file.
  switch (typeFile.toLocaleLowerCase()) {
    case 'png': {
      accpt = 'image/png';
      break;
    }
    case 'jpeg': {
      accpt = 'image/jpg';
      break;
    }
    case 'jpg': {
      accpt = 'image/jpg';
      break;
    }

    case 'pdf': {
      accpt = 'application/pdf';
      break;
    }

    default:
      accpt = 'application/octet-stream';
      break;
  }

  return accpt;
};
