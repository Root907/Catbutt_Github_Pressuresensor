#!/bin/bash
# GitHub 推送脚本
# 使用方法：修改下面的 YOUR_USERNAME 和 REPO_NAME，然后运行此脚本

# ============================================
# 请修改以下两个变量：
# ============================================
GITHUB_USERNAME="YOUR_USERNAME"  # 改成你的GitHub用户名
REPO_NAME="TestMat"               # 改成你想要的仓库名

# ============================================
# 推送命令
# ============================================
echo "准备推送到 GitHub..."
echo "仓库地址: https://github.com/${GITHUB_USERNAME}/${REPO_NAME}"

# 检查是否已有远程仓库
if git remote | grep -q "^origin$"; then
    echo "检测到已有 origin 远程仓库，正在更新..."
    git remote set-url origin https://github.com/${GITHUB_USERNAME}/${REPO_NAME}.git
else
    echo "添加远程仓库..."
    git remote add origin https://github.com/${GITHUB_USERNAME}/${REPO_NAME}.git
fi

# 重命名分支为 main
git branch -M main

# 推送代码
echo ""
echo "正在推送到 GitHub..."
echo "如果提示输入用户名和密码："
echo "  - Username: 输入你的 GitHub 用户名"
echo "  - Password: 输入你的 Personal Access Token (不是密码！)"
echo ""
git push -u origin main

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ 推送成功！"
    echo "访问: https://github.com/${GITHUB_USERNAME}/${REPO_NAME}"
else
    echo ""
    echo "❌ 推送失败，请检查："
    echo "1. 是否已在 GitHub 上创建了仓库: ${REPO_NAME}"
    echo "2. 用户名是否正确: ${GITHUB_USERNAME}"
    echo "3. 是否使用了正确的 Personal Access Token"
fi

