import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OtpService {
  private apiUrl = 'https://localhost:44369/api'; 

  constructor(private http: HttpClient) { }

  generateOtp(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/Otp/generate`);
  }

  validateOtp(otp: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Otp/validate`,{otp},{responseType: 'text'});
  }
}
