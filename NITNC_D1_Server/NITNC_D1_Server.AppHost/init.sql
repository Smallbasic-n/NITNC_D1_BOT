--
-- PostgreSQL database dump
--

-- Dumped from database version 17.2 (Debian 17.2-1.pgdg120+1)
-- Dumped by pg_dump version 17.2

-- Started on 2024-11-30 22:20:36 JST

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 3459 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 220 (class 1259 OID 24596)
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetRoleClaims" OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 24582)
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetRoles" (
    "Id" text NOT NULL,
    "Name" text,
    "NormalizedName" text,
    "ConcurrencyStamp" text
);


ALTER TABLE public."AspNetRoles" OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 24608)
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" text NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetUserClaims" OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 24620)
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" text NOT NULL
);


ALTER TABLE public."AspNetUserLogins" OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 24632)
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" text NOT NULL,
    "RoleId" text NOT NULL
);


ALTER TABLE public."AspNetUserRoles" OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 24649)
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" text NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);


ALTER TABLE public."AspNetUserTokens" OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 24589)
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AspNetUsers" (
    "Id" text NOT NULL,
    "UserName" text,
    "NormalizedUserName" text,
    "Email" text,
    "NormalizedEmail" text,
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" text,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);


ALTER TABLE public."AspNetUsers" OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 32801)
-- Name: ChankQuestions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ChankQuestions" (
    id integer NOT NULL,
    japanese character varying NOT NULL,
    english character varying,
    answer character varying
);


ALTER TABLE public."ChankQuestions" OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 32813)
-- Name: FactbookQuestions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."FactbookQuestions" (
    id integer NOT NULL,
    japanese character varying,
    answer character varying
);


ALTER TABLE public."FactbookQuestions" OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 32832)
-- Name: KiyomoriAssignments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."KiyomoriAssignments" (
    id integer NOT NULL,
    subjectname character varying NOT NULL,
    workname character varying NOT NULL,
    range character varying NOT NULL,
    deadline date NOT NULL
);


ALTER TABLE public."KiyomoriAssignments" OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 32825)
-- Name: KiyomoriSchedules; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."KiyomoriSchedules" (
    id integer NOT NULL,
    subjectname character varying NOT NULL,
    "Date" date NOT NULL,
    starts integer NOT NULL,
    ends integer NOT NULL
);


ALTER TABLE public."KiyomoriSchedules" OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 32854)
-- Name: MatsudairaDatas; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."MatsudairaDatas" (
    id integer NOT NULL,
    accountid character varying NOT NULL,
    chank integer DEFAULT 0 NOT NULL,
    factbook integer DEFAULT 0 NOT NULL,
    fristname character varying NOT NULL,
    givenname character varying NOT NULL,
    email character varying NOT NULL
);


ALTER TABLE public."MatsudairaDatas" OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 24577)
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- TOC entry 3444 (class 0 OID 24596)
-- Dependencies: 220
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- TOC entry 3442 (class 0 OID 24582)
-- Dependencies: 218
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
\.


--
-- TOC entry 3445 (class 0 OID 24608)
-- Dependencies: 221
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- TOC entry 3446 (class 0 OID 24620)
-- Dependencies: 222
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- TOC entry 3447 (class 0 OID 24632)
-- Dependencies: 223
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
\.


--
-- TOC entry 3448 (class 0 OID 24649)
-- Dependencies: 224
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
\.


--
-- TOC entry 3443 (class 0 OID 24589)
-- Dependencies: 219
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") FROM stdin;
\.


--
-- TOC entry 3449 (class 0 OID 32801)
-- Dependencies: 225
-- Data for Name: ChankQuestions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ChankQuestions" (id, japanese, english, answer) FROM stdin;
\.


--
-- TOC entry 3450 (class 0 OID 32813)
-- Dependencies: 226
-- Data for Name: FactbookQuestions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."FactbookQuestions" (id, japanese, answer) FROM stdin;
\.


--
-- TOC entry 3452 (class 0 OID 32832)
-- Dependencies: 228
-- Data for Name: KiyomoriAssignments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."KiyomoriAssignments" (id, subjectname, workname, range, deadline) FROM stdin;
\.


--
-- TOC entry 3451 (class 0 OID 32825)
-- Dependencies: 227
-- Data for Name: KiyomoriSchedules; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."KiyomoriSchedules" (id, subjectname, "Date", starts, ends) FROM stdin;
\.


--
-- TOC entry 3453 (class 0 OID 32854)
-- Dependencies: 229
-- Data for Name: MatsudairaDatas; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."MatsudairaDatas" (id, accountid, chank, factbook, fristname, givenname, email) FROM stdin;
\.


--
-- TOC entry 3441 (class 0 OID 24577)
-- Dependencies: 217
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
00000000000000_CreateIdentitySchema	8.0.11
\.


--
-- TOC entry 3270 (class 2606 OID 24602)
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- TOC entry 3262 (class 2606 OID 24588)
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- TOC entry 3273 (class 2606 OID 24614)
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- TOC entry 3276 (class 2606 OID 24626)
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- TOC entry 3279 (class 2606 OID 24638)
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- TOC entry 3281 (class 2606 OID 24655)
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- TOC entry 3266 (class 2606 OID 24595)
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- TOC entry 3260 (class 2606 OID 24581)
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- TOC entry 3289 (class 2606 OID 32838)
-- Name: KiyomoriAssignments assignments_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."KiyomoriAssignments"
    ADD CONSTRAINT assignments_pk PRIMARY KEY (id);


--
-- TOC entry 3283 (class 2606 OID 32807)
-- Name: ChankQuestions chank_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ChankQuestions"
    ADD CONSTRAINT chank_pk PRIMARY KEY (id);


--
-- TOC entry 3285 (class 2606 OID 32819)
-- Name: FactbookQuestions factbook_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."FactbookQuestions"
    ADD CONSTRAINT factbook_pk PRIMARY KEY (id);


--
-- TOC entry 3287 (class 2606 OID 32831)
-- Name: KiyomoriSchedules schedule_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."KiyomoriSchedules"
    ADD CONSTRAINT schedule_pk PRIMARY KEY (id);


--
-- TOC entry 3264 (class 1259 OID 24666)
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- TOC entry 3268 (class 1259 OID 24661)
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- TOC entry 3271 (class 1259 OID 24663)
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- TOC entry 3274 (class 1259 OID 24664)
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- TOC entry 3277 (class 1259 OID 24665)
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- TOC entry 3263 (class 1259 OID 24662)
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- TOC entry 3267 (class 1259 OID 24667)
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- TOC entry 3290 (class 2606 OID 24603)
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3291 (class 2606 OID 24615)
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3292 (class 2606 OID 24627)
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3293 (class 2606 OID 24639)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- TOC entry 3294 (class 2606 OID 24644)
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- TOC entry 3295 (class 2606 OID 24656)
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


-- Completed on 2024-11-30 22:20:37 JST

--
-- PostgreSQL database dump complete
--

