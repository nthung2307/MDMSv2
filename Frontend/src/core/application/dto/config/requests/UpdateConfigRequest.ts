export class UpdateConfigRequest {
  banner: string;
  facebook: string;
  youtube: string;
  address: string;
  hotline: string;
  email: string;

  constructor(arg: {
    banner: string;
    facebook: string;
    youtube: string;
    address: string;
    hotline: string;
    email: string;
  }) {
    this.banner = arg.banner;
    this.facebook = arg.facebook;
    this.youtube = arg.youtube;
    this.address = arg.address;
    this.hotline = arg.hotline;
    this.email = arg.email;
  }
}
