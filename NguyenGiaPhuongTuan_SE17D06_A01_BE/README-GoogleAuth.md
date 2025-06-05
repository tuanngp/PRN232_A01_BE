# Google Authentication API

## Tổng quan
API hỗ trợ 2 phương thức đăng nhập bằng Google:

1. **OAuth Flow**: Redirect user đến Google để đăng nhập
2. **ID Token Validation**: Nhận Google ID Token từ frontend và validate

## Endpoints

### 1. OAuth Flow (cho web application)

#### GET `/api/auth/google-signin`
- Redirect user đến Google OAuth consent screen
- Sau khi user đồng ý, Google sẽ redirect về `/api/auth/google-callback`
- Phù hợp cho server-side rendering apps

#### GET `/api/auth/google-callback`
- Xử lý callback từ Google OAuth
- Tự động tạo user nếu chưa tồn tại
- Redirect về frontend với cookies được set

### 2. ID Token Validation (RECOMMENDED cho SPA/Mobile)

#### POST `/api/auth/google-login-token-only`
```json
{
  "idToken": "google_id_token_here"
}
```

**Response**: Giống hệt như login thường
```json
{
  "success": true,
  "message": "Đăng nhập bằng Google thành công.",
  "data": {
    "accessToken": "jwt_token_here",
    "refreshToken": "refresh_token_here", 
    "accessTokenExpires": "2024-01-01T12:00:00Z",
    "refreshTokenExpires": "2024-01-08T12:00:00Z",
    "user": {
      "accountId": 1,
      "accountName": "User Name",
      "accountEmail": "user@gmail.com",
      "accountRole": "Staff"
    }
  }
}
```

#### POST `/api/auth/google-login`
- Tương tự như trên nhưng có thể set cookie nếu gửi header `X-Set-Cookie: true`
- Mặc định không set cookie

## Cách Frontend sử dụng

### 1. Với Google Sign-In JavaScript API
```javascript
// 1. Load Google Sign-In library
// 2. Initialize
google.accounts.id.initialize({
  client_id: 'YOUR_GOOGLE_CLIENT_ID',
  callback: handleGoogleSignIn
});

// 3. Handle sign in
async function handleGoogleSignIn(response) {
  try {
    const result = await fetch('/api/auth/google-login-token-only', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        idToken: response.credential
      })
    });
    
    const data = await result.json();
    
    if (data.success) {
      // Lưu token vào localStorage/sessionStorage
      localStorage.setItem('accessToken', data.data.accessToken);
      localStorage.setItem('refreshToken', data.data.refreshToken);
      
      // Redirect hoặc update UI
      window.location.href = '/dashboard';
    }
  } catch (error) {
    console.error('Google login failed:', error);
  }
}
```

### 2. Với react-google-login
```javascript
import { GoogleLogin } from 'react-google-login';

const handleGoogleSuccess = async (response) => {
  try {
    const result = await fetch('/api/auth/google-login-token-only', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        idToken: response.tokenId
      })
    });
    
    const data = await result.json();
    // Handle success...
  } catch (error) {
    // Handle error...
  }
};

// Component
<GoogleLogin
  clientId="YOUR_GOOGLE_CLIENT_ID"
  buttonText="Đăng nhập bằng Google"
  onSuccess={handleGoogleSuccess}
  onFailure={handleGoogleFailure}
  cookiePolicy={'single_host_origin'}
/>
```

## Configuration

### appsettings.json
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

## Tự động tạo User
- Khi user đăng nhập lần đầu bằng Google, hệ thống sẽ tự động tạo account mới
- Default role: `Staff`
- Password: random (user không thể dùng để đăng nhập bằng email/password)

## Testing
Sử dụng file `google-auth-test.http` để test các endpoints.

## Security Notes
- Google ID Token được validate với Google servers
- Chỉ accept token từ configured Client ID
- Token có thời hạn ngắn (thường 1 giờ)
- Refresh token được lưu để gia hạn session 