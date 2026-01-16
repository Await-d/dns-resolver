# DNS Resolver

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19.x-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**Multi-ISP DNS Resolution Management Panel**

English | [简体中文](./README.md)

</div>

---

## 📖 Introduction

DNS Resolver is a powerful multi-ISP DNS resolution management panel that provides:

- 🔍 **DNS Query** - Query and compare DNS resolution results from multiple ISPs simultaneously
- 🌐 **DNS Record Management** - Integrated with 23 mainstream DNS providers, full CRUD operations
- 🔄 **DDNS** - Automatically detect public IP changes and update DNS records
- 🎨 **Modern UI** - Cyberpunk-style interface with i18n support, fully responsive

## ✨ Features

### DNS Resolution Query
- Query 8 ISPs simultaneously (China Telecom, Unicom, Mobile, Alibaba, Tencent, Baidu, Google, Cloudflare)
- One-click comparison of multi-ISP resolution results
- Support A, AAAA, CNAME, MX, TXT, NS, SOA record types
- Display response time for each query

### DNS Record Management
- Integrated with 23 mainstream DNS provider APIs
- Support query, add, update, delete DNS records
- Batch operations support
- Persistent user provider configuration

### DDNS Dynamic DNS
- Auto-detect public IP (IPv4/IPv6 support)
- Scheduled tasks for automatic IP change detection
- Update only when IP changes to reduce API calls
- Multiple IP detection sources

## 🛠️ Tech Stack

### Backend
- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM (SQLite)
- **DDD Architecture** - Domain-Driven Design
- **JWT Authentication** - Secure user authentication

### Frontend
- **React 19** - Latest React framework
- **TypeScript 5** - Type safety
- **Vite 6** - Fast build tool
- **TanStack Query** - Data fetching management
- **Tailwind CSS 4** - Utility-first CSS framework
- **i18next** - Internationalization

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/Await-d/dns-resolver.git
cd dns-resolver

# Start the service
docker compose up -d
```

The service will be available at `http://localhost:7010`.

**Default credentials**: admin / admin123

### Manual Deployment

#### Requirements
- .NET SDK 10.0+
- Node.js 22.x LTS+
- pnpm 9.x (recommended)

#### Backend

```bash
cd backend/src/DnsResolver.Api
dotnet restore
dotnet run
```

#### Frontend

```bash
cd frontend
pnpm install
pnpm dev
```

## 📦 Supported Providers

### DNS ISPs

| ISP | ID | Primary DNS | Secondary DNS |
|-----|-----|-------------|---------------|
| China Telecom | telecom | 202.96.128.86 | 202.96.128.166 |
| China Unicom | unicom | 221.5.88.88 | 221.6.4.66 |
| China Mobile | mobile | 211.136.192.6 | 211.136.112.50 |
| Alibaba DNS | aliyun | 223.5.5.5 | 223.6.6.6 |
| Tencent DNS | tencent | 119.29.29.29 | 119.28.28.28 |
| Baidu DNS | baidu | 180.76.76.76 | - |
| Google DNS | google | 8.8.8.8 | 8.8.4.4 |
| Cloudflare | cloudflare | 1.1.1.1 | 1.0.0.1 |

### DNS Providers (23)

<details>
<summary>Click to expand full list</summary>

| Provider | ID | Description |
|----------|-----|-------------|
| Alibaba Cloud DNS | alidns | Alibaba Cloud DNS |
| Alibaba Cloud ESA | aliesa | Alibaba Cloud Edge Security |
| Tencent Cloud DNS | tencentcloud | Tencent Cloud DNS |
| Tencent Cloud EdgeOne | edgeone | Tencent Cloud Edge Security |
| DNSPod | dnspod | DNSPod DNS |
| Cloudflare | cloudflare | Cloudflare DNS |
| Huawei Cloud DNS | huaweicloud | Huawei Cloud DNS |
| Baidu Cloud DNS | baiducloud | Baidu Cloud DNS |
| GoDaddy | godaddy | GoDaddy Domain Service |
| Namecheap | namecheap | Namecheap Domain Service |
| Namesilo | namesilo | Namesilo Domain Service |
| Porkbun | porkbun | Porkbun Domain Service |
| Vercel | vercel | Vercel DNS |
| Gcore | gcore | Gcore DNS |
| NS1 | nsone | NS1 DNS |
| Dynadot | dynadot | Dynadot Domain Service |
| DNSLA | dnsla | DNSLA DNS |
| Dynv6 | dynv6 | Dynv6 Dynamic DNS |
| Spaceship | spaceship | Spaceship Domain Service |
| TrafficRoute | trafficroute | TrafficRoute DNS |
| Eranet | eranet | Eranet Domain Service |
| NowCN | nowcn | NowCN Domain Service |
| Callback | callback | Custom Callback |

</details>

## 📁 Project Structure

```
dns-resolver/
├── backend/                          # Backend project
│   └── src/
│       ├── DnsResolver.Domain/       # Domain layer - Core business logic
│       ├── DnsResolver.Application/  # Application layer - Use case orchestration
│       ├── DnsResolver.Infrastructure/ # Infrastructure layer - External dependencies
│       └── DnsResolver.Api/          # Presentation layer - HTTP API
├── frontend/                         # Frontend project
│   └── src/
│       ├── components/               # Reusable components
│       ├── pages/                    # Page components
│       ├── services/                 # API calls
│       ├── hooks/                    # Custom Hooks
│       ├── contexts/                 # React Context
│       ├── i18n/                     # Internationalization config
│       └── types/                    # TypeScript types
├── docker-compose.yml                # Docker Compose config
├── Dockerfile                        # Docker build file
└── README.md                         # Project documentation
```

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | Production |
| `Jwt__SecretKey` | JWT secret key | Random generated |
| `Cors__Origins` | Allowed CORS origins | http://localhost:5173 |

### Backend Configuration

Edit `backend/src/DnsResolver.Api/appsettings.json`:

```json
{
  "DnsSettings": {
    "QueryTimeoutSeconds": 5,
    "MaxRetries": 2
  },
  "Jwt": {
    "SecretKey": "your-secret-key",
    "Issuer": "DnsResolver",
    "Audience": "DnsResolver"
  }
}
```

## 📸 Screenshots

<details>
<summary>Click to view screenshots</summary>

- Home - DNS Record Management
- DNS Resolution Comparison
- DDNS Task Management
- Provider Configuration

</details>

## 🤝 Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the [MIT](LICENSE) License.

## 🙏 Acknowledgments

- DNS provider integration inspired by [ddns-go](https://github.com/jeessy2/ddns-go)
- Thanks to all contributors

## 📮 Contact

- Project Homepage: https://github.com/Await-d/dns-resolver
- Issue Tracker: https://github.com/Await-d/dns-resolver/issues

---

<div align="center">

**If this project helps you, please give it a ⭐ Star!**

</div>
