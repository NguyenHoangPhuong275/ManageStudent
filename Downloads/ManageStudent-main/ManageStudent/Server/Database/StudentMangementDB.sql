CREATE DATABASE StudentManager;
GO
USE StudentManager;
CREATE TABLE Users (
    Username NVARCHAR(50) PRIMARY KEY,
    Password NVARCHAR(50),
    Role NVARCHAR(10) 
);

INSERT INTO Users VALUES ('admin', '123', 'ADMIN');
INSERT INTO Users VALUES ('user1', '123', 'USER');


CREATE TABLE Students (
    StudentID NVARCHAR(20) PRIMARY KEY,
    FullName NVARCHAR(100),
    Class NVARCHAR(50)
);

INSERT INTO Students VALUES ('SV001', 'Le Ngoc Chau Anh', 'CNTT');
INSERT INTO Students VALUES ('SV002', 'Nguyen Van An', 'CNTT');
INSERT INTO Students VALUES ('SV003', 'Dao Gia Bao', 'CNTT');
INSERT INTO Students VALUES ('SV004', 'Ngo Thu Cuc', 'CNTT');
INSERT INTO Students VALUES ('SV005', 'Hoang Van Tuan Dat', 'CNTT');
INSERT INTO Students VALUES ('SV006', 'Le Trung Dung', 'CNTT');
INSERT INTO Students VALUES ('SV007', 'Vu Thi Thuy Duong', 'CNTT');
INSERT INTO Students VALUES ('SV008', 'Pham Thi My Duyen', 'CNTT');
INSERT INTO Students VALUES ('SV009', 'Do Hoang Giang', 'CNTT');
INSERT INTO Students VALUES ('SV010', 'Bui Thi Thu Hien', 'CNTT');
INSERT INTO Students VALUES ('SV011', 'Dang Nhat Huy', 'CNTT');
INSERT INTO Students VALUES ('SV012', 'Ngo Thi Kim Huyen', 'CNTT');
INSERT INTO Students VALUES ('SV013', 'Tran Lam', 'CNTT');
INSERT INTO Students VALUES ('SV014', 'Le Ha My Linh', 'CNTT');
INSERT INTO Students VALUES ('SV015', 'Tran Ngo My Linh', 'CNTT');
INSERT INTO Students VALUES ('SV016', 'Tran Hoai Nam', 'CNTT');
INSERT INTO Students VALUES ('SV017', 'Nguyen Tran Trung Nam', 'CNTT');
INSERT INTO Students VALUES ('SV018', 'Le Hoang Phi Ngan', 'CNTT');
INSERT INTO Students VALUES ('SV019', 'Chau Ngoc Tuyet Ngan', 'CNTT');
INSERT INTO Students VALUES ('SV020', 'Vu Van Nghinh', 'CNTT');
INSERT INTO Students VALUES ('SV021', 'Do Tan Phat', 'CNTT');
INSERT INTO Students VALUES ('SV022', 'Ngo Duy Phuc', 'CNTT');
INSERT INTO Students VALUES ('SV023', 'Le Anh Quynh', 'CNTT');
INSERT INTO Students VALUES ('SV024', 'Nguyen Thi Truc Quynh', 'CNTT');
INSERT INTO Students VALUES ('SV025', 'Ha Van Sa', 'CNTT');
INSERT INTO Students VALUES ('SV026', 'Bui Ngoc Sang', 'CNTT');
INSERT INTO Students VALUES ('SV027', 'Le Tan Tai', 'CNTT');
INSERT INTO Students VALUES ('SV028', 'Do Tan Tu', 'CNTT');
INSERT INTO Students VALUES ('SV029', 'Truong Thanh Tung', 'CNTT');
INSERT INTO Students VALUES ('SV030', 'Tran Thu Uyen', 'CNTT');
INSERT INTO Students VALUES ('SV031', 'Ngo Thuy Uyen', 'CNTT');
INSERT INTO Students VALUES ('SV032', 'Ngo Huu Vinh', 'CNTT');
INSERT INTO Students VALUES ('SV033', 'Tran Huu Vinh', 'CNTT');
INSERT INTO Students VALUES ('SV034', 'Pham Xuyen', 'CNTT');
INSERT INTO Students VALUES ('SV035', 'Ho Ngoc Nhu Y', 'CNTT');


