import { HttpClient } from '@angular/common/http';
import { ConditionalExpr } from '@angular/compiler';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.scss']
})
export class TestErrorComponent implements OnInit {
  baseUrl = environment.apiUrl;
  validationErrors: any;

  constructor(private http: HttpClient) { }

  ngOnInit()  {
  }

  get404error(){
    this.http.get(this.baseUrl + 'products/42').subscribe(response => {
        console.log(response);
    }, error => {
      console.log(error);
    });
  }

  get500error(){
    this.http.get(this.baseUrl + 'buggy/servererror').subscribe(response => {
        console.log(response);
    }, error => {
      console.log(error);
    });
  }

  get400error(){
    this.http.get(this.baseUrl + 'buggy/badrequest').subscribe(response => {
        console.log(response);
    }, error => {
      console.log(error);
      
      
    });
  }

  get400Validationerror(){
    this.http.get(this.baseUrl + 'products/fortytwo').subscribe(response => {
        console.log(response);
    }, error => {
      console.log(error);
      this.validationErrors = error.errors;
    });
  }

}
