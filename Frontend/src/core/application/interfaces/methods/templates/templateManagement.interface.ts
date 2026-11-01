export interface NotificationTemplateDto {
  id: number | string;
  code: string;
  name: string;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
  createdTime: string;
  modifiedTime?: string;
}

export interface CreateNotificationTemplateRequest {
  code: string;
  name: string;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdateNotificationTemplateRequest {
  name: string;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  description?: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface PreviewNotificationTemplateRequest {
  code?: string;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  customVariables?: Record<string, string>;
}

export interface PreviewNotificationTemplateResponse {
  subject: string;
  bodyHtml: string;
}

export interface StatementPdfTemplateDto {
  id: number | string;
  code: string;
  name: string;
  headerTitle: string;
  subHeaderTitle: string;
  companyName: string;
  companyAddress: string;
  contactPhone: string;
  logoBase64?: string;
  paperSize: 'A5_LANDSCAPE' | 'A4_PORTRAIT' | string;
  displayColumnsJson: string;
  footerNotes?: string;
  requireDualSignatures: boolean;
  isDefault: boolean;
  isActive: boolean;
  createdTime: string;
  modifiedTime?: string;
}

export interface CreateStatementPdfTemplateRequest {
  code: string;
  name: string;
  headerTitle: string;
  subHeaderTitle: string;
  companyName: string;
  companyAddress: string;
  contactPhone: string;
  logoBase64?: string;
  paperSize: 'A5_LANDSCAPE' | 'A4_PORTRAIT' | string;
  displayColumnsJson: string;
  footerNotes?: string;
  requireDualSignatures: boolean;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdateStatementPdfTemplateRequest {
  name: string;
  headerTitle: string;
  subHeaderTitle: string;
  companyName: string;
  companyAddress: string;
  contactPhone: string;
  logoBase64?: string;
  paperSize: 'A5_LANDSCAPE' | 'A4_PORTRAIT' | string;
  displayColumnsJson: string;
  footerNotes?: string;
  requireDualSignatures: boolean;
  isDefault: boolean;
  isActive: boolean;
}

export interface PreviewStatementPdfTemplateRequest {
  headerTitle: string;
  subHeaderTitle: string;
  companyName: string;
  companyAddress: string;
  contactPhone: string;
  logoBase64?: string;
  paperSize: string;
  displayColumnsJson: string;
  footerNotes?: string;
  requireDualSignatures: boolean;
}
