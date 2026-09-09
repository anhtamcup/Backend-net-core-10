# Partner Access Key Authentication

Các API dành cho đối tác yêu cầu hai header:

\`\`\`http
X-Access-Key: s3_uat_xxxxxxxxx
X-Secret-Key: xxxxxxxxxxxxxxxxxxxxxxxxx
\`\`\`

Chỉ gọi API qua HTTPS. IT chịu trách nhiệm allowlist IP tại firewall, load balancer
hoặc reverse proxy; ứng dụng không kiểm tra IP.

## Khởi tạo database

Áp dụng migration trước khi cấp partner:

\`\`\`powershell
dotnet tool run dotnet-ef database update
\`\`\`

## Tạo partner

\`\`\`powershell
dotnet run --no-build -- partner create --code PARTNER_B --name "Partner B"
\`\`\`

Khi chạy từ thư mục publish:

\`\`\`powershell
dotnet S3.Gateway.dll partner create --code PARTNER_B --name "Partner B"
\`\`\`

Partner code là duy nhất, không phân biệt chữ hoa/thường và chỉ gồm chữ, số,
dấu gạch ngang hoặc gạch dưới.

Nếu DBA muốn tạo Partner và credential full quyền hoàn toàn bằng SQL, chỉnh bốn
biến cấu hình rồi chạy \`Administration/Sql/CreatePartnerWithFullAccess.sql\`.
Script chạy trong một transaction và chỉ trả secret key ở kết quả lần chạy đó.

## Sinh access key và secret key

\`\`\`powershell
dotnet run --no-build -- credential create --partner PARTNER_B --environment uat --expires 2027-12-31
\`\`\`

Credential mặc định được cấp toàn bộ scope. Chỉ truyền \`--scopes\` khi cần
giới hạn quyền cho một credential. Giá trị \`--expires\` là tùy chọn.

Secret key được sinh bằng bộ sinh số mật mã và chỉ xuất hiện một lần trên terminal.
Database chỉ lưu SHA-256 của secret. Nếu mất secret, hãy sinh credential mới và
thu hồi credential cũ; không thể đọc lại secret từ database.

## Scope hỗ trợ

- \`payment.qr.create\`
- \`payment.investigation\`
- \`payment.refund\`
- \`ekyc.deeplink.create\`
- \`ekyc.merchant.create\`
- \`ekyc.merchant.update\`
- \`ekyc.merchant.query\`

## Endpoint không dùng Partner Access Key

Các callback do Napas gọi tiếp tục xác minh chữ ký Napas:

- \`/payment/napas/apg/notification\`
- \`/napas/callbackstatus\`
- \`/ekyc/pmnp/callbackstatus\`

## Checklist cấp mới

1. Nhận PartnerCode, tên đối tác, môi trường, scope và IP từ đơn vị tích hợp.
2. IT allowlist IP đúng môi trường và đúng endpoint.
3. Chạy lệnh tạo partner.
4. Chạy lệnh tạo credential.
5. Chuyển access/secret key qua kênh bảo mật; không gửi chung với tài liệu API.
6. Kiểm thử UAT với request có và không có header.
7. Cấp credential Production riêng sau khi UAT đạt.
