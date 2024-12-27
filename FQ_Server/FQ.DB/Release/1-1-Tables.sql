CREATE TABLE Accounts (
 Login TEXT NOT NULL UNIQUE,
 HASHPassword TEXT NOT NULL,
 UserID uuid NOT NULL UNIQUE,
 IsMain BOOLEAN NOT NULL,
 AuthMethod INT NOT NULL,
 Token CHAR(88) NOT NULL,
 LastAction TIMESTAMP NOT NULL,
 Email TEXT NOT NULL
);

CREATE TABLE TempAccounts (
 Email TEXT NOT NULL,
 HASHPassword TEXT NOT NULL,
 ConfirmCode CHAR(6) NOT NULL,
 CreationDate TIMESTAMP NOT NULL
);

CREATE TABLE Users (
 ID uuid NOT NULL UNIQUE,
 GroupID uuid NOT NULL,
 Name TEXT NOT NULL,
 Title TEXT NOT NULL,
 Role INT NOT NULL, --0 = user; 1 = GroupOwner
 Coins INT NOT NULL,
 Img TEXT NOT NULL,
 Status INT NOT NULL
);

CREATE TABLE Groups (
 ID uuid NOT NULL UNIQUE,
 Name TEXT NOT NULL,
 Img TEXT NOT NULL
);

CREATE TABLE Rewards (
 ID uuid NOT NULL UNIQUE,
 GroupID uuid NOT NULL,
 Title TEXT NOT NULL,
 Description TEXT NOT NULL,
 Cost INT NOT NULL,
 Img TEXT NOT NULL,
 Creator uuid NOT NULL,
 AvailableFor uuid NOT NULL,
 Status INT NOT NULL, --available = 0; purchased = 1; handed = 2; Deleted = 1000
 CreationDate TIMESTAMP NOT NULL,
 PurchaseDate TIMESTAMP NOT NULL,
 HandedDate TIMESTAMP NOT NULL,
 ModificationTime TIMESTAMP NOT NULL
);

CREATE TABLE Tasks (
 ID uuid NOT NULL UNIQUE,
 Type INT NOT NULL,
 Name TEXT NOT NULL,
 Description TEXT,
 Cost INT NOT NULL,
 Penalty INT NOT NULL,
 AvailableUntil TIMESTAMP NOT NULL,
 SolutionTime INTERVAL NOT NULL,
 SpeedBonus INT NOT NULL,
 OwnerGroup uuid NOT NULL,
 AvailableFor uuid[] NOT NULL,
 Creator uuid NOT NULL,
 Executor uuid NOT NULL,
 Status INT NOT NULL,
 CreationDate TIMESTAMP NOT NULL,
 CompletionDate TIMESTAMP NOT NULL,
 ModificationTime TIMESTAMP NOT NULL
);

CREATE TABLE HistoryEvents (
 ID uuid NOT NULL UNIQUE,
 GroupID uuid NOT NULL,
 CreationDate TIMESTAMP NOT NULL,
 ItemType INT NOT NULL,
 MessageType INT NOT NULL,
 Visability INT NOT NULL,
 AvailableFor uuid[] NOT NULL,
 TargetItem uuid NOT NULL,
 Doer uuid NOT NULL
);

CREATE TABLE AccountDevices (
 UserID uuid NOT NULL,
 DeviceID VARCHAR(1000) NOT NULL,
 RegToken VARCHAR(1000) NOT NULL,
 UNIQUE(UserID, DeviceID)
);

CREATE TABLE AccountNotificationSubs(
 UserID uuid NOT NULL UNIQUE,
 IsSubscribed BOOL
);

CREATE TABLE Subscriptions (
 PurchaseToken TEXT NOT NULL UNIQUE,
 GroupID uuid NOT NULL,
 OrderID TEXT NOT NULL, -- (можно не хранить, но обезопасимся)
 Months INT NOT NULL,  --кол-во месяцев
 InnerState INT NOT NULL, --0 = purchased; 1 = acivated; 2 = expired; 3 = voided
 VoidedSource INT NOT NULL, -- (можно не хранить, но обезопасимся)  0 = User; 1 = Developer; 2 = Google
 VoidedReason INT NOT NULL, -- (можно не хранить, но обезопасимся) 0 = Other; 1 = Remorse; 2 = Not_received; 3 = Defective; 4 = Accidental_purchase; 5 = Fraud; 6 = Friendly_fraud; 7 = Chargeback
 ModificationTime TIMESTAMP NOT NULL
);
