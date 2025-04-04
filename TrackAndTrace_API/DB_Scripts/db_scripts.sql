--================ Role ===============--
CREATE TABLE roles(
	id INT PRIMARY KEY IDENTITY(1,1),
	name nvarchar(100) NULL,
);

--================ Add Roles ===============--

INSERT INTO roles (name) VALUES
('SUPER_ADMIN'),
('CHAIRMAN'),
('COMPANY_ADMIN'),
('PROJECT_ADMIN'),
('FINANCE_ADMIN'),
('SITE_ENGINEER'),
('GENERAL_STAFF');

--================ Organization ===============--
CREATE TABLE organization (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
	phone varchar(45) NULL,
	logo nvarchar(max) NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_date datetime NOT NULL,
	updated_date datetime NULL
);

CREATE INDEX IX_organization_name ON organization(name);

--================ Add Organization ===============--
INSERT INTO organization (code, name, created_date) VALUES ('D64 Org', 'Dimension64 Org', GETDATE());

--================ Company ===============--
CREATE TABLE company (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
	phone varchar(45) NULL,
	logo nvarchar(max) NULL,
    organization_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_date datetime NOT NULL,
	updated_date datetime NULL,
    FOREIGN KEY (organization_id) REFERENCES organization(id) ON DELETE CASCADE,
);

CREATE INDEX IX_company_name ON company(name);

--================ Add Company ===============--
INSERT INTO company (code, name, organization_id, created_date) VALUES ('D64', 'Dimension64', (SELECT id FROM organization WHERE name = 'Dimension64 Org'), GETDATE());

--================ Designation ===============--
CREATE TABLE designation (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    role_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

CREATE INDEX IX_designation_company_name ON designation(company_id, name);

--================ Add Designation ===============--
INSERT INTO designation (code, name, company_id, role_id, created_by, created_date) VALUES 
('SA', 'Super Admin', (select id from company where code = 'D64'), (select id from roles where name = 'SUPER_ADMIN'), 1, GETDATE());

--================ Users ===============--
CREATE TABLE users(
	id INT PRIMARY KEY IDENTITY(1,1),
	uuid uniqueidentifier NOT NULL DEFAULT NEWID(),
	code nvarchar(50) NOT NULL,
	name nvarchar(100) NOT NULL,
	email nvarchar(50) NOT NULL,
	phone nvarchar(20) NULL,
	password nvarchar(max) NOT NULL,
	company_id int NOT NULL,
	designation_id int NOT NULL,
    device_user_id INT DEFAULT NULL,
	active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
	FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE,
	FOREIGN KEY (designation_id) REFERENCES designation(id) ON DELETE NO ACTION
);

CREATE INDEX IX_users_company_email ON dbo.users(company_id, email);

--================ Add User ===============--

INSERT INTO users
           (code
		   ,name
           ,email
           ,phone
           ,password
		   ,company_id
		   ,designation_id
           ,created_by
           ,created_date)
     VALUES
           ('S001'
		   ,'Super Admin User'
           ,'dimension64@gmail.com'
           ,'8105009454'
           ,'b8/Wr4NjrhwclX671xgfaw=='
           ,(select id from company where code = 'D64')
           ,(select id from designation where code = 'SA')
           ,1
           ,GETDATE());
		   
--================ Add Menu, Page & Menu Page mapping ===============--
CREATE TABLE page (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    url NVARCHAR(MAX) NULL,
    created_date DATETIME DEFAULT GETDATE(),
    updated_date DATETIME NULL
);

CREATE TABLE menu (
    id INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX) NULL,
    icon TEXT NULL,
    created_date DATETIME DEFAULT GETDATE(),
    updated_date DATETIME NULL
);

CREATE TABLE menu_page_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    menu_id INT,
    page_id INT,
	mapping_order INT,
    created_date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (menu_id) REFERENCES menu(id) ON DELETE CASCADE,
    FOREIGN KEY (page_id) REFERENCES page(id) ON DELETE CASCADE
);

--================ Master Tables ===============--

CREATE TABLE brand (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_brand_company_name ON brand(company_id, name);

--==============================--

--CREATE TABLE category (
--    id INT PRIMARY KEY IDENTITY(1,1),
--    code varchar(50) NOT NULL,
--    name varchar(100) NOT NULL,
--    company_id int NOT NULL,
--    active_flag bit NOT NULL DEFAULT 1,
--	delete_flag bit NOT NULL DEFAULT 0,
--	created_by int NOT NULL,
--	created_date datetime NOT NULL,
--	updated_by int NULL,
--	updated_date datetime NULL,
--    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
--);

--CREATE INDEX IX_category_company_name ON category(company_id, name);

--==============================--

--CREATE TABLE sub_category (
--    id INT PRIMARY KEY IDENTITY(1,1),
--    code varchar(50) NOT NULL,
--    name varchar(100) NOT NULL,
--    category_id int NOT NULL,
--    company_id int NOT NULL,
--    active_flag bit NOT NULL DEFAULT 1,
--	delete_flag bit NOT NULL DEFAULT 0,
--	created_by int NOT NULL,
--	created_date datetime NOT NULL,
--	updated_by int NULL,
--	updated_date datetime NULL,
--    FOREIGN KEY (category_id) REFERENCES category(id) ON DELETE NO ACTION,
--    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
--);

--CREATE INDEX IX_sub_category_company_category_name ON sub_category(company_id, category_id, name);

--==============================--

CREATE TABLE uom (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_uom_company_name ON uom(company_id, name);

--==============================--

CREATE TABLE machinery (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    quantity decimal(5,2) NOT NULL,
    in_house bit NOT NULL DEFAULT 1,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_machinery_company_name ON machinery(company_id, name);

--==============================--

CREATE TABLE injury (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_injury_company_name ON injury(company_id, name);

--==============================--

CREATE TABLE shift (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_shift_company_name ON shift(company_id, name);

--==============================--

CREATE TABLE weather (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_weather_company_name ON weather(company_id, name);

--==============================--

CREATE TABLE labour_type (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_labour_type_company_name ON labour_type(company_id, name);

--==============================--

CREATE TABLE warehouse (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    address NVARCHAR(MAX) NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_warehouse_company_name ON warehouse(company_id, name);

--==============================--

CREATE TABLE subcontractor (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    address NVARCHAR(MAX) NULL,
    labour_type_id int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (labour_type_id) REFERENCES labour_type(id) ON DELETE CASCADE,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_subcontractor_company_labour_type_name ON subcontractor(company_id, labour_type_id, name);

--==============================--

CREATE TABLE material (
    id INT PRIMARY KEY IDENTITY(1,1),
	code varchar(50) NOT NULL,
	name varchar(100) NOT NULL,
	description nvarchar(max) NULL,
	cost decimal(18, 2) NOT NULL DEFAULT 0,
	uom_id int NOT NULL,
	type varchar(10) NULL,
	company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (uom_id) REFERENCES uom(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_material_company_name ON material(company_id, name);

--==============================--

CREATE TABLE material_brand_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    material_id int NOT NULL,
    brand_id int NOT NULL,
    FOREIGN KEY (material_id) REFERENCES material(id) ON DELETE NO ACTION,
    FOREIGN KEY (brand_id) REFERENCES brand(id) ON DELETE NO ACTION
);

CREATE INDEX IX_material_brand_mapping ON material_brand_mapping(material_id, brand_id);

--==============================--

CREATE TABLE differentiator (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(100) NOT NULL,
    name varchar(100) NOT NULL,
    material_id int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (material_id) REFERENCES material(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_differentiator_company_material_name ON differentiator(company_id, material_id, name);

--==============================--

CREATE TABLE differentiator_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    differentiator_id int NOT NULL,
    value varchar(1000) NOT NULL,
    FOREIGN KEY (differentiator_id) REFERENCES differentiator(id) ON DELETE NO ACTION
);

CREATE INDEX IX_differentiator_mapping ON differentiator_mapping(differentiator_id, value);

--==============================--

--CREATE TABLE specification (
--    id INT PRIMARY KEY IDENTITY(1,1),
--    code varchar(50) NOT NULL,
--    name varchar(100) NOT NULL,
--    company_id int NOT NULL,
--    active_flag bit NOT NULL DEFAULT 1,
--	delete_flag bit NOT NULL DEFAULT 0,
--	created_by int NOT NULL,
--	created_date datetime NOT NULL,
--	updated_by int NULL,
--	updated_date datetime NULL,
--    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
--);

--CREATE INDEX IX_specification_company_name ON specification(company_id, name);

--==============================--

--CREATE TABLE specification_differentiator_mapping (
--    id INT PRIMARY KEY IDENTITY(1,1),
--    specification_id int NOT NULL,
--    differentiator_id int NOT NULL,
--    value varchar(100) NOT NULL,
--    FOREIGN KEY (specification_id) REFERENCES specification(id) ON DELETE NO ACTION,
--    FOREIGN KEY (differentiator_id) REFERENCES differentiator(id) ON DELETE NO ACTION
--);

--CREATE INDEX IX_specification_differentiator ON specification_differentiator_mapping(specification_id, differentiator_id);

--===============================--

CREATE TABLE company_role_menu_page_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    company_id INT,
    role_id INT,
    menu_id INT,
    page_id INT,
	mapping_order INT,
    created_date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE,
    FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
    FOREIGN KEY (menu_id) REFERENCES menu(id) ON DELETE CASCADE,
    FOREIGN KEY (page_id) REFERENCES page(id) ON DELETE CASCADE
);

--==============================--

CREATE TABLE project (
    id INT PRIMARY KEY IDENTITY(1,1),
	code varchar(50) NOT NULL,
	name varchar(100) NOT NULL,
	description nvarchar(max) NULL,
	address nvarchar(max) NULL,
	client nvarchar(max) NULL,
	cost decimal(18, 2) NOT NULL DEFAULT 0,
	start_date datetime NULL,
    end_date datetime NULL,
	work_hours decimal(18, 2) NOT NULL DEFAULT 0,
	admin_id int NOT NULL,
	manager_id int NOT NULL,
	engineer_id int NOT NULL,
	logo nvarchar(max) NULL,
    warehouse_id int NOT NULL,
	company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (admin_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (manager_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (engineer_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (warehouse_id) REFERENCES warehouse(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_project_company_name ON project(company_id, name);

--==============================--

CREATE TABLE project_user_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
	project_id int NOT NULL,
	user_id int NOT NULL,
	delete_flag bit NOT NULL DEFAULT 0,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION
);

CREATE INDEX IX_project_user ON project_user_mapping(project_id, user_id);

--===============================--

CREATE TABLE work_flow (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_work_flow_company_name ON work_flow(company_id, name);

--===============================--

CREATE TABLE work_flow_project_user_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    work_flow_id int NOT NULL,
    project_id int NOT NULL,
    user_id int NOT NULL,
    order_id int NOT NULL,
    is_supersede bit NOT NULL DEFAULT 0,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (work_flow_id) REFERENCES work_flow(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION
);

CREATE INDEX IX_work_flow_project_user ON work_flow_project_user_mapping(work_flow_id, project_id, user_id);

--===============================--

CREATE TABLE bulk_import_details (
    id INT PRIMARY KEY IDENTITY(1,1),
    name varchar(100) NOT NULL,
    company_id int NOT NULL,
    total int NOT NULL DEFAULT 0,
    success int NOT NULL DEFAULT 0,
    failed int NOT NULL DEFAULT 0,
    failed_records NVARCHAR(MAX) NULL,
    status varchar(100) NULL,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE CASCADE
);

CREATE INDEX IX_bulk_import_details_company_name ON bulk_import_details(company_id, name);

--===============================--

CREATE TABLE activity (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    estimated_days int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_activity_company_name ON activity(company_id, name);

--==============================--

CREATE TABLE activity_project_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
	activity_id int NOT NULL,
	project_id int NOT NULL,
    FOREIGN KEY (activity_id) REFERENCES activity(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION
);

CREATE INDEX IX_activity_project ON activity_project_mapping(activity_id, project_id);

--===============================--

CREATE TABLE task (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    activity_id int NOT NULL,
    uom_id int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (activity_id) REFERENCES activity(id) ON DELETE NO ACTION,
    FOREIGN KEY (uom_id) REFERENCES uom(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_task_company_name ON task(company_id, name);

--==============================--

CREATE TABLE task_project_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
	task_id int NOT NULL,
	project_id int NOT NULL,
    FOREIGN KEY (task_id) REFERENCES task(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION
);

CREATE INDEX IX_task_project ON task_project_mapping(task_id, project_id);

--===============================--

CREATE TABLE sub_task (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    activity_id int NOT NULL,
    task_id int NOT NULL,
    uom_id int NOT NULL,
	is_prime bit NOT NULL DEFAULT 0,
    estimated_days int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (activity_id) REFERENCES activity(id) ON DELETE NO ACTION,
    FOREIGN KEY (task_id) REFERENCES task(id) ON DELETE NO ACTION,
    FOREIGN KEY (uom_id) REFERENCES uom(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_sub_task_company_name ON sub_task(company_id, name);

--==============================--

CREATE TABLE sub_task_project_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
	sub_task_id int NOT NULL,
	project_id int NOT NULL,
	start_date varchar(50) NOT NULL,
	execution_days int NOT NULL DEFAULT 0,
	end_date varchar(50) NOT NULL,
    installation_cost decimal(18, 2) NOT NULL DEFAULT 0,
	manpower_count int NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (sub_task_id) REFERENCES sub_task(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION
);

CREATE INDEX IX_sub_task_project ON sub_task_project_mapping(sub_task_id, project_id);

--==============================--

CREATE TABLE trx_work_flow_approval_status (
    id INT PRIMARY KEY IDENTITY(1,1),
    request_id int NOT NULL,
    wf_project_user_id int NOT NULL,
    user_id int NOT NULL,
    order_id int NOT NULL,
    is_supersede bit NOT NULL DEFAULT 0,
    status varchar(50) NULL,
    created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (request_id) REFERENCES bulk_import_details(id) ON DELETE NO ACTION,
    FOREIGN KEY (wf_project_user_id) REFERENCES work_flow_project_user_mapping(id) ON DELETE NO ACTION,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_work_flow_approval_status ON trx_work_flow_approval_status(wf_project_user_id, user_id);

--===============================--

CREATE TABLE project_level_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
    name varchar(100) NOT NULL,
    parent_id int NOT NULL DEFAULT 0,
    project_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION
);

CREATE INDEX IX_project_level_code ON project_level_mapping(project_id, code);

--==============================--

CREATE TABLE manpower (
    id INT PRIMARY KEY IDENTITY(1,1),
	code varchar(50) NOT NULL,
	name varchar(100) NOT NULL,
	designation_id int NOT NULL,
	engineer_id int NOT NULL,
	charge_hand_id int NULL,
	gang_leader_id int NULL,
	subcontractor_id int NULL,
    rating decimal(18, 2) NULL DEFAULT 0,
	company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
	delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (designation_id) REFERENCES designation(id) ON DELETE NO ACTION,
    FOREIGN KEY (engineer_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (charge_hand_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (gang_leader_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (subcontractor_id) REFERENCES subcontractor(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_manpower_company_name ON manpower(company_id, name);

--==============================--

CREATE TABLE manpower_project_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
	manpower_id int NOT NULL,
	project_id int NOT NULL,
	delete_flag bit NOT NULL DEFAULT 0,
    FOREIGN KEY (manpower_id) REFERENCES manpower(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION
);

CREATE INDEX IX_manpower_project ON manpower_project_mapping(manpower_id, project_id);

--==============================--

CREATE TABLE trx_daily_activity_item (
    id INT PRIMARY KEY IDENTITY(1,1),
    activity_id int NOT NULL,
    task_id int NOT NULL,
    sub_task_id int NOT NULL,
    favourite_by nvarchar(max) NULL,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (activity_id) REFERENCES activity(id) ON DELETE NO ACTION,
    FOREIGN KEY (task_id) REFERENCES task(id) ON DELETE NO ACTION,
    FOREIGN KEY (sub_task_id) REFERENCES sub_task(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_activity_task_sub_task_item ON trx_daily_activity_item(activity_id, task_id, sub_task_id);

--===============================--

CREATE TABLE trx_daily_activity_details (
    id INT PRIMARY KEY IDENTITY(1,1),
    activity_item_id int NOT NULL,
    project_id int NOT NULL,
    project_level_id int NOT NULL,
    quantity decimal(18, 2) NOT NULL DEFAULT 0,
    progress decimal(18, 2) NOT NULL DEFAULT 0,
    shift_id int NOT NULL,
    hrs_spent decimal(18, 2) NOT NULL DEFAULT 0,
    labour_type_id int NOT NULL,
    subcontractor_id int NULL,
    weather_id int NULL,
    remarks nvarchar(max) NULL,
    status varchar(100) NULL,
    is_draft bit NOT NULL DEFAULT 0,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (activity_item_id) REFERENCES trx_daily_activity_item(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION,
    FOREIGN KEY (project_level_id) REFERENCES project_level_mapping(id) ON DELETE NO ACTION,
    FOREIGN KEY (shift_id) REFERENCES shift(id) ON DELETE NO ACTION,
    FOREIGN KEY (labour_type_id) REFERENCES labour_type(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_daily_activity_details ON trx_daily_activity_details(activity_item_id, project_id, is_draft);

--===============================--

CREATE TABLE trx_daily_activity_manpower (
    id INT PRIMARY KEY IDENTITY(1,1),
    daily_activity_id int NOT NULL,
    manpower_id int NOT NULL,
	designation_id int NOT NULL,
	engineer_id int NOT NULL,
	charge_hand_id int NULL,
	gang_leader_id int NULL,
	subcontractor_id int NULL,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (daily_activity_id) REFERENCES trx_daily_activity_details(id) ON DELETE NO ACTION,
    FOREIGN KEY (manpower_id) REFERENCES manpower(id) ON DELETE NO ACTION,
    FOREIGN KEY (designation_id) REFERENCES designation(id) ON DELETE NO ACTION,
    FOREIGN KEY (engineer_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_daily_activity_manpower ON trx_daily_activity_manpower(daily_activity_id, manpower_id);

--===============================--

CREATE TABLE trx_daily_activity_material (
    id INT PRIMARY KEY IDENTITY(1,1),
    daily_activity_id int NOT NULL,
    material_id int NOT NULL,
    quantity decimal(18, 2) NOT NULL DEFAULT 0,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (daily_activity_id) REFERENCES trx_daily_activity_details(id) ON DELETE NO ACTION,
    FOREIGN KEY (material_id) REFERENCES material(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_daily_activity_material ON trx_daily_activity_material(daily_activity_id, material_id);

--===============================--

CREATE TABLE trx_daily_activity_machinery (
    id INT PRIMARY KEY IDENTITY(1,1),
    daily_activity_id int NOT NULL,
    machinery_id int NOT NULL,
    quantity decimal(18, 2) NOT NULL DEFAULT 0,
    start_time varchar(20) NULL,
    end_time varchar(20) NULL,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (daily_activity_id) REFERENCES trx_daily_activity_details(id) ON DELETE NO ACTION,
    FOREIGN KEY (machinery_id) REFERENCES machinery(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_trx_daily_activity_machinery ON trx_daily_activity_machinery(daily_activity_id, machinery_id);

--===============================--

CREATE TABLE user_attendance (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id int NOT NULL,
    latitude varchar(50) NOT NULL,
    longitude varchar(50) NOT NULL,
    address nvarchar(max) NOT NULL,
    image nvarchar(max) NOT NULL,
    attendance_type varchar(20) NOT NULL,
    attendance_timestamp varchar(50) NOT NULL,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_user_attendance ON user_attendance(company_id, user_id, attendance_type);

--===============================--

CREATE TABLE space_management (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
	name varchar(100) NOT NULL,
    capacity int NULL DEFAULT 0,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_space_management ON space_management(company_id, name);

--===============================--

CREATE TABLE meeting (
    id INT PRIMARY KEY IDENTITY(1,1),
    title varchar(200) NOT NULL,
    agenda nvarchar(max) NOT NULL,
    meeting_date datetime NOT NULL,
    start_time varchar(50) NULL,
    end_time varchar(50) NULL,
    meeting_url nvarchar(max) NULL,
    status varchar(20) NOT NULL,
    color varchar(50) NULL,
    company_id int NOT NULL,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_meeting ON meeting(company_id, title, status);

--===============================--

CREATE TABLE meeting_attendee_detail (
    id INT PRIMARY KEY IDENTITY(1,1),
    meeting_id int NOT NULL,
    attendee_id int NOT NULL,
    FOREIGN KEY (meeting_id) REFERENCES meeting(id) ON DELETE NO ACTION,
    FOREIGN KEY (attendee_id) REFERENCES users(id) ON DELETE NO ACTION
);

CREATE INDEX IX_meeting_attendee ON meeting_attendee_detail(meeting_id, attendee_id);

--===============================--

CREATE TABLE meeting_assigned_task (
    id INT PRIMARY KEY IDENTITY(1,1),
    meeting_attendee_id int NOT NULL,
    task nvarchar(max) NOT NULL,
	due_date datetime NOT NULL,
    status varchar(20) NOT NULL,
    company_id int NOT NULL,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (meeting_attendee_id) REFERENCES meeting_attendee_detail(id) ON DELETE NO ACTION,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_meeting_task ON meeting_assigned_task(company_id, meeting_attendee_id);

--===============================--

CREATE TABLE activity_milestone (
    id INT PRIMARY KEY IDENTITY(1,1),
    code varchar(50) NOT NULL,
	name varchar(100) NOT NULL,
    start_date varchar(50) NOT NULL,
	end_date varchar(50) NOT NULL,
    budget_cost decimal(18, 2) NOT NULL DEFAULT 0,
    project_id int NOT NULL,
    company_id int NOT NULL,
    active_flag bit NOT NULL DEFAULT 1,
    delete_flag bit NOT NULL DEFAULT 0,
	created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (company_id) REFERENCES company(id) ON DELETE NO ACTION
);

CREATE INDEX IX_activity_milestone ON activity_milestone(company_id, name);

--===============================--

CREATE TABLE activity_milestone_mapping (
    id INT PRIMARY KEY IDENTITY(1,1),
    activity_milestone_id int NOT NULL,
    activity_id int NOT NULL,
    start_date varchar(50) NOT NULL,
	end_date varchar(50) NOT NULL,
    cost decimal(18, 2) NOT NULL DEFAULT 0,
    FOREIGN KEY (activity_milestone_id) REFERENCES activity_milestone(id) ON DELETE NO ACTION,
    FOREIGN KEY (activity_id) REFERENCES activity(id) ON DELETE NO ACTION
);

CREATE INDEX IX_activity_milestone_mapping ON activity_milestone_mapping(activity_milestone_id, activity_id);

--==============================--

CREATE TABLE indent (
    id INT PRIMARY KEY IDENTITY(1,1),
	project_id int NOT NULL,
	request_id int NOT NULL,
    indent_type varchar(20) NOT NULL,
    indent_no varchar(100) NOT NULL,
    indent_date varchar(50) NOT NULL,
	status varchar(100) NOT NULL,
	po_number varchar(100) NULL,
    po_file_url nvarchar(max) NULL,
    created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION,
    FOREIGN KEY (request_id) REFERENCES bulk_import_details(id) ON DELETE NO ACTION
);

CREATE INDEX IX_indent ON indent(project_id, request_id, indent_no);

--==============================--

CREATE TABLE indent_material (
    id INT PRIMARY KEY IDENTITY(1,1),
	indent_id int NOT NULL,
	material_id int NOT NULL,
    quantity decimal(5,2) NOT NULL,
    brand_id int NOT NULL,
    lead_days int NULL DEFAULT 0,
    delivery_date varchar(50) NULL,
    supply_cost decimal(18, 2) NULL DEFAULT 0,
    remarks nvarchar(max) NULL,
    FOREIGN KEY (indent_id) REFERENCES indent(id) ON DELETE NO ACTION,
    FOREIGN KEY (material_id) REFERENCES material(id) ON DELETE NO ACTION,
    FOREIGN KEY (brand_id) REFERENCES brand(id) ON DELETE NO ACTION
);

CREATE INDEX IX_indent_material ON indent_material(indent_id, material_id);

--==============================--

CREATE TABLE indent_material_differentiator (
    id INT PRIMARY KEY IDENTITY(1,1),
	indent_material_id int NOT NULL,
	differentiator_mapping_id int NOT NULL,
    FOREIGN KEY (indent_material_id) REFERENCES indent_material(id) ON DELETE NO ACTION,
    FOREIGN KEY (differentiator_mapping_id) REFERENCES differentiator_mapping(id) ON DELETE NO ACTION
);

CREATE INDEX IX_indent_material_differentiator ON indent_material_differentiator(indent_material_id, differentiator_mapping_id);

--==============================--

CREATE TABLE project_material (
    id INT PRIMARY KEY IDENTITY(1,1),
	project_id int NOT NULL,
	material_id int NOT NULL,
    actual_quantity decimal(5,2) NOT NULL,
    available_quantity decimal(5,2) NULL,
    created_by int NOT NULL,
	created_date datetime NOT NULL,
	updated_by int NULL,
	updated_date datetime NULL,
    FOREIGN KEY (project_id) REFERENCES project(id) ON DELETE NO ACTION,
    FOREIGN KEY (material_id) REFERENCES material(id) ON DELETE NO ACTION
);

CREATE INDEX IX_indent_material ON indent_material(indent_id, material_id);

--===============================--Till above published
--=============== Insert Workflow ================--

/*
INSERT INTO work_flow (code, name, company_id, created_by, created_date) VALUES
('WF001', 'BOQ', (select id from company where code = 'TAT'), 1, GETDATE());

INSERT INTO work_flow (code, name, company_id, created_by, created_date) VALUES
('MAI', 'Master Indent', (select id from company where code = 'TAT'), 1, GETDATE()),
('VAI', 'Variant Indent', (select id from company where code = 'TAT'), 1, GETDATE()),
('NTI', 'Non-Tender Indent', (select id from company where code = 'TAT'), 1, GETDATE());
*/

--===============================--
/*
{
  "email": "dimension64@gmail.com",
  "password": "Password"
}
*/