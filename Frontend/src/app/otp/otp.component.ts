import { Component } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { OtpService } from '../otp.service';

@Component({
  selector: 'app-otp',
  templateUrl: './otp.component.html',
  styleUrls: ['./otp.component.scss']
})
export class OtpComponent {
  otpReceived: boolean = false;
  otpMessage: string = '';
  enteredOTP: string = '';

  constructor(private otpService: OtpService, private toastr: ToastrService) { }

  ngOnInit() {
    this.generateOTP();
  }

  generateOTP() {
    this.otpService.generateOtp().subscribe(
      (response: any) => {
        this.toastr.success(`Your OTP: ${response.otp}`, 'Success', { timeOut: 55000 }); 
      },
      (error: any) => {
        console.error('Error generating OTP:', error);
        this.toastr.error('Error generating OTP');
      }
    );
  }

  validateOTP() {
    this.otpService.validateOtp(this.enteredOTP).subscribe(
      (response: any) => {
      
        if (response && response.toLowerCase() === 'otp is valid'){
          console.log("Response", response.text);
          this.toastr.success('OTP is valid', 'Success');
        } else {
          this.toastr.error('OTP is invalid', 'Error');
        }
      },
      (error: any) => {
        console.error('Error validating OTP:', error);
        this.toastr.error('OTP validation failed', 'Error');
      }
    );
  }
  
  
  
  
}
